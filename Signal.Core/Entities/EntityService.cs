using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Signal.Core.Contacts;
using Signal.Core.Exceptions;
using Signal.Core.Sharing;
using Signal.Core.Storage;
using Signal.Core.Users;

namespace Signal.Core.Entities;

internal class EntityService : IEntityService
{
    private readonly ISharingService sharingService;
    private readonly IAzureStorageDao storageDao;
    private readonly IAzureStorage storage;

    public EntityService(
        ISharingService sharingService,
        IAzureStorageDao storageDao,
        IAzureStorage storage)
    {
        this.sharingService = sharingService ?? throw new ArgumentNullException(nameof(sharingService));
        this.storageDao = storageDao ?? throw new ArgumentNullException(nameof(storageDao));
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    public async Task<Dictionary<string, IEnumerable<IUser>>> EntityUsersAsync(IEnumerable<string> entityIds, CancellationToken cancellationToken)
    {
        var entityIdsList = entityIds.ToList();
        var assignedDevicesUsers = await storageDao.AssignedUsersAsync(
            entityIdsList,
            cancellationToken);
        var assignedUserIds = assignedDevicesUsers.Values.SelectMany(i => i).Distinct().ToList();
        var assignedUsers = new Dictionary<string, IUser>();
        foreach (var userId in assignedUserIds)
        {
            var assignedUser = await storageDao.UserAsync(userId, cancellationToken);
            if (assignedUser != null)
                assignedUsers.Add(assignedUser.UserId, assignedUser);
        }

        var entityUsersDictionary = new Dictionary<string, IEnumerable<IUser>>();
        foreach (var entityId in entityIdsList)
        {
            var users = new List<IUser>();
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

    public async Task<IEnumerable<IEntity>> AllAsync(string userId, CancellationToken cancellationToken = default) => 
        await this.storageDao.UserEntitiesAsync(userId, cancellationToken);

    public async Task<IEntity?> GetAsync(string entityId, CancellationToken cancellationToken = default) => 
        await this.storageDao.GetAsync(entityId, cancellationToken);

    public async Task<string> UpsertAsync(string userId, string? entityId, Func<string, IEntity> entityFunc, CancellationToken cancellationToken)
    {
        // Check if existing entity was requested but not assigned
        var exists = false;
        if (entityId != null)
        {
            exists = await storageDao.EntityExistsAsync(entityId, cancellationToken);
            var isAssigned = await storageDao.IsUserAssignedAsync(
                userId, entityId, cancellationToken);

            if (exists && !isAssigned)
                throw new ExpectedHttpException(HttpStatusCode.NotFound);
        }

        // Create entity
        var id = entityId ?? await GenerateEntityIdAsync(cancellationToken);
        await storage.UpsertAsync(
            entityFunc(id),
            cancellationToken);

        // Assign to user if creating entity
        if (!exists)
        {
            await this.sharingService.AssignToUserAsync(
                userId,
                id,
                cancellationToken);
        }

        return id;
    }

    public async Task<IEnumerable<IContact>> ContactsAsync(string entityId, CancellationToken cancellationToken) => 
        await this.storageDao.ContactsAsync(entityId, cancellationToken);

    public async Task<IEnumerable<IContact>> ContactsAsync(IEnumerable<string> entityIds, CancellationToken cancellationToken) => 
        await this.storageDao.ContactsAsync(entityIds, cancellationToken);

    private async Task<string> GenerateEntityIdAsync(CancellationToken cancellationToken = default)
    {
        var newId = Guid.NewGuid().ToString();
        while (await storageDao.EntityExistsAsync(newId, cancellationToken))
            newId = Guid.NewGuid().ToString();
        return newId;
    }

    public async Task RemoveAsync(string userId, string entityId, CancellationToken cancellationToken)
    {
        // Validate assigned
        if (!(await this.IsUserAssignedAsync(userId, entityId, cancellationToken)))
            throw new ExpectedHttpException(HttpStatusCode.NotFound);

        // TODO: Remove assignments for all users (since entity doesn't exist anymore)

        // Remove entity
        await storage.RemoveEntityAsync(entityId, cancellationToken);
    }

    public Task<bool> IsUserAssignedAsync(string userId, string id, CancellationToken cancellationToken) =>
        storageDao.IsUserAssignedAsync(userId, id, cancellationToken);
}