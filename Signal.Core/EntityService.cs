using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Signal.Core.Exceptions;
using Signal.Core.Storage;
using Signal.Core.Users;

namespace Signal.Core
{
    internal class EntityService : IEntityService
    {
        private readonly IAzureStorageDao storageDao;
        private readonly IAzureStorage storage;

        public EntityService(
            IAzureStorageDao storageDao, 
            IAzureStorage storage)
        {
            this.storageDao = storageDao ?? throw new ArgumentNullException(nameof(storageDao));
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public async Task<Dictionary<string, IEnumerable<IUserTableEntity>>> EntityUsersAsync(TableEntityType entityType, IEnumerable<string> entityIds, CancellationToken cancellationToken)
        {
            var entityIdsList = entityIds.ToList();
            var assignedDevicesUsers = await this.storageDao.AssignedUsersAsync(
                entityType, 
                entityIdsList,
                cancellationToken);
            var assignedUserIds = assignedDevicesUsers.Values.SelectMany(i => i).Distinct().ToList();
            var assignedUsers = new Dictionary<string, IUserTableEntity>();
            foreach (var userId in assignedUserIds)
            {
                var assignedUser = await this.storageDao.UserAsync(userId, cancellationToken);
                if (assignedUser != null)
                    assignedUsers.Add(assignedUser.RowKey, assignedUser);
            }

            var entityUsersDictionary = new Dictionary<string, IEnumerable<IUserTableEntity>>();
            foreach (var entityId in entityIdsList)
            {
                var users = new List<IUserTableEntity>();
                if (assignedDevicesUsers.TryGetValue(entityId, out var assignedDeviceUserIds))
                {
                    foreach (var assignedDeviceUserId in assignedDeviceUserIds)
                    {
                        if (assignedUsers.TryGetValue(assignedDeviceUserId, out var assignedDeviceUser))
                        {
                            users.Add(assignedDeviceUser);
                        }
                    }
                }
                entityUsersDictionary.Add(entityId, users);
            }

            return entityUsersDictionary;
        }

        public async Task<string> UpsertAsync<TEntity>(string userId, string? entityId, TableEntityType entityType, string tableName, Func<string, TEntity> entityFunc, CancellationToken cancellationToken)
            where TEntity : ITableEntity
        {
            // Check if existing entity was requested but not assigned
            var exists = false;
            if (entityId != null)
            {
                exists = (await this.storageDao.EntitiesByRowKeysAsync(
                    tableName, new[] {entityId}, cancellationToken)).Any();
                var isAssigned = await this.storageDao.IsUserAssignedAsync(
                    userId, entityType, entityId, cancellationToken);

                if (exists && !isAssigned)
                    throw new ExpectedHttpException(HttpStatusCode.NotFound);
            }

            // Create entity
            var id = entityId ?? await this.GenerateEntityIdAsync(tableName, cancellationToken);
            await this.storage.CreateOrUpdateItemAsync(
                tableName,
                entityFunc(id),
                cancellationToken);

            // Assign if creating entity
            if (!exists)
            {
                await this.storage.CreateOrUpdateItemAsync(
                    ItemTableNames.UserAssignedEntity(entityType),
                    new UserAssignedEntityTableEntry(
                        userId,
                        id),
                    cancellationToken);
            }

            return id;
        }

        private async Task<string> GenerateEntityIdAsync(string tableName, CancellationToken cancellationToken = default)
        {
            var newId = Guid.NewGuid().ToString();
            while ((await this.storageDao.EntitiesByRowKeysAsync(tableName, new[] { newId }, cancellationToken)).Any())
                newId = Guid.NewGuid().ToString();
            return newId;
        }

        public async Task RemoveByIdAsync(string tableName, string rowKey, CancellationToken cancellationToken)
        {
            var entities = await this.storageDao.EntitiesByRowKeysAsync(tableName, new[] { rowKey }, cancellationToken);
            var entitiesList = entities as IList<ITableEntityKey> ?? entities.ToList();
            if (!entitiesList.Any()) return;

            foreach (var item in entitiesList)
                await this.storage.DeleteItemAsync(tableName, item.PartitionKey, item.RowKey, cancellationToken);
        }

        public Task<bool> IsUserAssignedAsync(string userId, TableEntityType entityType, string id, CancellationToken cancellationToken) =>
            this.storageDao.IsUserAssignedAsync(
                userId, entityType, id, cancellationToken);
    }
}
