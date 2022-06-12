using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Signal.Core.Contacts;
using Signal.Core.Entities;
using Signal.Core.Sharing;
using Signal.Core.Storage;
using Signal.Core.Storage.Blobs;
using Signal.Core.Users;
using BlobInfo = Signal.Core.Storage.Blobs.BlobInfo;

namespace Signal.Infrastructure.AzureStorage.Tables;

internal class AzureStorageDao : IAzureStorageDao
{
    private readonly IAzureStorageClientFactory clientFactory;


    public AzureStorageDao(IAzureStorageClientFactory clientFactory)
    {
        this.clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
    }


    public async Task<IEnumerable<IContactHistoryItem>> ContactHistoryAsync(
        IContactPointer contactPointer,
        TimeSpan duration,
        CancellationToken cancellationToken)
    {
        try
        {
            var client =
                await this.clientFactory.GetTableClientAsync(ItemTableNames.ContactsHistory, cancellationToken);
            var history = client.QueryAsync<AzureContactHistoryItem>(entry =>
                entry.PartitionKey == $"{contactPointer.EntityId}-{contactPointer.ChannelName}-{contactPointer.ContactName}");

            // Limit to 30 days
            // TODO: Move this check to BLL
            var correctedDuration = duration;
            if (correctedDuration > TimeSpan.FromDays(30))
                correctedDuration = TimeSpan.FromDays(30);

            // Fetch all until reaching requested duration
            var items = new List<IContactHistoryItem>();
            var startDateTime = DateTime.UtcNow - correctedDuration;
            await foreach (var data in history)
            {
                var item = AzureContactHistoryItem.ToContactHistoryItem(data);
                if (item.Timestamp < startDateTime)
                    break;

                items.Add(item);
            }

            return items;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return Enumerable.Empty<IContactHistoryItem>();
        }
    }

    public async Task<string?> UserIdByEmailAsync(string userEmail, CancellationToken cancellationToken)
    {
        try
        {
            var client = await this.clientFactory.GetTableClientAsync(ItemTableNames.Users, cancellationToken);
            var query = client.QueryAsync<AzureUser>(u => u.Email == userEmail);
            await foreach (var match in query)
                return match.RowKey;
            return null;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }
    
    public async Task<IUser?> UserAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await this.clientFactory.GetTableClientAsync(ItemTableNames.Users, cancellationToken);
            return AzureUser.ToUser((await client.GetEntityAsync<AzureUser>(
                UserSources.GoogleOauth,
                userId,
                cancellationToken: cancellationToken)).Value);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<IEnumerable<IEntity>> UserEntitiesAsync(string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userAssignedEntities = await this.UserAssignedAsync(userId, cancellationToken);
            var client = await this.clientFactory.GetTableClientAsync(ItemTableNames.Entities, cancellationToken);
            var entitiesQuery = client.QueryAsync<AzureEntity>(
                RowsWithKeysAnyFilter(userAssignedEntities.Select(uae => uae.EntityId)),
                cancellationToken: cancellationToken);
            var entities = new List<IEntity>();
            await foreach (var entity in entitiesQuery)
                if (entity != null)
                    entities.Add(AzureEntity.ToEntity(entity));
            return entities;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return Enumerable.Empty<IEntity>();
        }
    }

    public async Task<IEntity?> GetAsync(string entityId, CancellationToken cancellationToken)
    {
        try
        {
            var client = await this.clientFactory.GetTableClientAsync(ItemTableNames.Entities, cancellationToken);
            var entitiesQuery = client.QueryAsync<AzureEntity>(e => e.RowKey == entityId);
            await foreach (var entity in entitiesQuery)
                if (entity != null)
                    return AzureEntity.ToEntity(entity);
            return null;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<IEnumerable<IContact>> ContactsAsync(
        string entityId,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = await this.clientFactory.GetTableClientAsync(ItemTableNames.Contacts, cancellationToken);
            var statesAsync = client.QueryAsync<AzureContact>(q => q.PartitionKey == entityId, cancellationToken: cancellationToken);
            var states = new List<IContact>();
            await foreach (var state in statesAsync)
                if (state != null)
                    states.Add(AzureContact.ToContact(state));
            return states;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return Enumerable.Empty<IContact>();
        }
    }

    public async Task<IEnumerable<IContact>> ContactsAsync(
        IEnumerable<string> entityIds, 
        CancellationToken cancellationToken)
    {
        var client = await this.clientFactory.GetTableClientAsync(ItemTableNames.Contacts, cancellationToken);
        var contactsQuery = client.QueryAsync<AzureContact>(PartitionsAnyFilter(entityIds), cancellationToken: cancellationToken);
        var states = new List<IContact>();
        await foreach (var state in contactsQuery)
            if (state != null)
                states.Add(AzureContact.ToContact(state));
        return states;
    }

    private static string PartitionsAnyFilter(IEnumerable<string> partitionKeys) =>
        $"({string.Join(" or", partitionKeys.Select(tl => $"(PartitionKey eq '{tl}')"))})";

    private static string RowsWithKeysAnyFilter(IEnumerable<string> rowKeys) => 
        $"({string.Join(" or", rowKeys.Select(tl => $"(RowKey eq '{tl}')"))})";

    public async Task<Stream> LoggingDownloadAsync(string blobName, CancellationToken cancellationToken)
    {
        var client = await this.clientFactory.GetAppendBlobClientAsync(BlobContainerNames.StationLogs, blobName, cancellationToken);
        return await client.OpenReadAsync(false, cancellationToken: cancellationToken);
    }

    public async Task<bool> EntityExistsAsync(string id, CancellationToken cancellationToken)
    {
        var client = await this.clientFactory.GetTableClientAsync(ItemTableNames.Entities, cancellationToken);
        var entityQuery = client.QueryAsync<AzureEntity>(e => e.RowKey == id, cancellationToken: cancellationToken);
        await foreach (var _ in entityQuery)
            return true;
        return false;
    }

    public async IAsyncEnumerable<IBlobInfo> LoggingListAsync(string stationId, CancellationToken cancellationToken)
    {
        var client = await this.clientFactory.GetBlobContainerClientAsync(BlobContainerNames.StationLogs, cancellationToken);
        var blobsQuery = client.GetBlobsByHierarchyAsync(prefix: stationId, cancellationToken: cancellationToken);
        if (blobsQuery == null)
            yield break;

        await foreach (var blobHierarchyItem in blobsQuery)
        {
            // Skip deleted
            if (blobHierarchyItem.Blob.Deleted) 
                continue;

            // Retrieve interesting data
            var info = new BlobInfo(blobHierarchyItem.Blob.Name)
            {
                CreatedTimeStamp = blobHierarchyItem.Blob.Properties.CreatedOn,
                LastModifiedTimeStamp = blobHierarchyItem.Blob.Properties.LastModified,
                Size = blobHierarchyItem.Blob.Properties.ContentLength
            };

            yield return info;
        }
    }

    public async Task<bool> IsUserAssignedAsync(string userId, string entityId, CancellationToken cancellationToken)
    {
        try
        {
            var client = await this.clientFactory.GetTableClientAsync(ItemTableNames.UserAssignedEntity, cancellationToken);
            var assignment = await client.GetEntityAsync<AzureUserAssignedEntitiesTableEntry>(
                userId, entityId, cancellationToken: cancellationToken);
            return assignment.Value != null;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }

    public async Task<IReadOnlyDictionary<string, IEnumerable<string>>> AssignedUsersAsync(
        IEnumerable<string> entityIds,
        CancellationToken cancellationToken)
    {
        try
        {
            var assignedUsers = new Dictionary<string, IEnumerable<string>>();
            var entityIdsList = entityIds.ToList();
            if (!entityIdsList.Any()) 
                return assignedUsers;

            var client = await this.clientFactory.GetTableClientAsync(ItemTableNames.UserAssignedEntity, cancellationToken);
            var assigned = client.QueryAsync<AzureUserAssignedEntitiesTableEntry>(
                RowsWithKeysAnyFilter(entityIdsList), cancellationToken: cancellationToken);

            await foreach (var entity in assigned)
            {
                if (assignedUsers.ContainsKey(entity.RowKey))
                    (assignedUsers[entity.RowKey] as List<string>)?.Add(entity.PartitionKey);
                else assignedUsers[entity.RowKey] = new List<string> {entity.PartitionKey};
            }

            return assignedUsers;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return ImmutableDictionary<string, IEnumerable<string>>.Empty;
        }
    }

    private async Task<IEnumerable<IUserAssignedEntity>> UserAssignedAsync(string userId, CancellationToken cancellationToken)
    {
        try
        {
            var client = await this.clientFactory.GetTableClientAsync(ItemTableNames.UserAssignedEntity, cancellationToken);
            var assigned = client.QueryAsync<AzureUserAssignedEntitiesTableEntry>(
                entry => entry.PartitionKey == userId,
                cancellationToken: cancellationToken);

            var assignedItems = new List<IUserAssignedEntity>();
            await foreach (var entity in assigned)
                assignedItems.Add(new UserAssignedEntity(userId, entity.RowKey));
            return assignedItems;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return Enumerable.Empty<IUserAssignedEntity>();
        }
    }
}