using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Signal.Core.Exceptions;
using Signal.Core.Storage;

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

        public async Task<string> UpsertEntityAsync<TEntity>(string userId, string? entityId, TableEntityType entityType, string tableName, Func<string, TEntity> entityFunc, CancellationToken cancellationToken)
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
            var id = entityId ?? Guid.NewGuid().ToString();
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
