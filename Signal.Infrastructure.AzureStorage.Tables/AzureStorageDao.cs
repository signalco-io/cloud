using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Signal.Core.Contacts;
using Signal.Core.Storage;
using Signal.Core.Storage.Blobs;
using Signal.Core.Users;
using BlobInfo = Signal.Core.Storage.Blobs.BlobInfo;
using ITableEntity = Azure.Data.Tables.ITableEntity;

namespace Signal.Infrastructure.AzureStorage.Tables;

internal class AzureStorageDao : IAzureStorageDao
{
    private readonly IAzureStorageClientFactory clientFactory;


    public AzureStorageDao(IAzureStorageClientFactory clientFactory)
    {
        this.clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
    }


    public async Task<IEnumerable<IContactHistoryItem>> GetDeviceStateHistoryAsync(
        string deviceId,
        string channelName,
        string contactName,
        TimeSpan duration,
        CancellationToken cancellationToken)
    {
        try
        {
            var client =
                await this.clientFactory.GetTableClientAsync(ItemTableNames.DevicesStatesHistory, cancellationToken);
            var history = client.QueryAsync<AzureDeviceStateHistoryTableEntity>(entry =>
                entry.PartitionKey == $"{deviceId}-{channelName}-{contactName}");

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
                if (data.Timestamp < startDateTime)
                    break;

                items.Add(data);
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
            return (await client.GetEntityAsync<AzureUser>(
                UserSources.GoogleOauth,
                userId,
                cancellationToken: cancellationToken)).Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<IEnumerable<IDashboardTableEntity>> DashboardsAsync(
        string userId,
        CancellationToken cancellationToken) =>
        await this.GetUserAssignedAsync<IDashboardTableEntity, AzureDashboardTableEntity>(
            userId,
            TableEntityType.Dashboard,
            ItemTableNames.Dashboards,
            null,
            dashboard => new DashboardTableEntity(
                dashboard.RowKey,
                dashboard.Name,
                dashboard.ConfigurationSerialized,
                dashboard.Timestamp?.DateTime),
            cancellationToken);
        

    public async Task<IEnumerable<IContact>> GetDeviceStatesAsync(
        IEnumerable<string> deviceIds,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = await this.clientFactory.GetTableClientAsync(ItemTableNames.DeviceStates, cancellationToken);
            var statesAsync = client.QueryAsync<AzureContact>(PartitionsAnyFilter(deviceIds));
            var states = new List<IContact>();
            await foreach (var state in statesAsync)
                if (state != null)
                    states.Add(state);
            return states;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return Enumerable.Empty<IContact>();
        }
    }
        
    private static string PartitionsAnyFilter(IEnumerable<string> partitionKeys) => 
        $"({string.Join(" or", partitionKeys.Select(tl => $"(PartitionKey eq '{tl}')"))})";

    private static string PartitionWithKeysAnyFilter(string partitionKey, IEnumerable<string> rowKeys) =>
        $"(PartitionKey eq '{partitionKey}') and" +
        $"({string.Join(" or", rowKeys.Select(tl => $"(RowKey eq '{tl}')"))})";

    private static string RowsWithKeysAnyFilter(IEnumerable<string> rowKeys) => 
        $"({string.Join(" or", rowKeys.Select(tl => $"(RowKey eq '{tl}')"))})";

    private async Task<IEnumerable<TEntity>> GetUserAssignedAsync<TEntity, TAzureTableEntity>(
        string userId, 
        string? partitionFilter,
        Func<TAzureTableEntity, TEntity> entityMap,
        CancellationToken cancellationToken) 
        where TAzureTableEntity : class, ITableEntity, new()
    {
        // Retrieve user assigned entities
        var userAssignedEntities = await this.UserAssignedAsync(userId, type, cancellationToken);

        // Select entity id's
        var assignedEntityIds = userAssignedEntities.Select(d => d.RowKey).ToList();
        if (!assignedEntityIds.Any())
            return Enumerable.Empty<TEntity>();

        // Query user assigned entities
        var client = await this.clientFactory.GetTableClientAsync(tableName, cancellationToken);
        var entityQuery = client.QueryAsync<TAzureTableEntity>(
            string.IsNullOrWhiteSpace(partitionFilter)
                ? RowsWithKeysAnyFilter(assignedEntityIds)
                : PartitionWithKeysAnyFilter(partitionFilter, assignedEntityIds),
            cancellationToken: cancellationToken);

        // Retrieve and map entities
        var entities = new List<TEntity>();
        await foreach (var entity in entityQuery)
            entities.Add(entityMap(entity));
        return entities;
    }

    public async Task<IEnumerable<ITableEntityKey>> EntitiesByRowKeysAsync(
        string tableName,
        IEnumerable<string> rowKeys,
        CancellationToken cancellationToken)
    {
        var client = await this.clientFactory.GetTableClientAsync(tableName, cancellationToken);
        var entityQuery = client.QueryAsync<TableEntity>(RowsWithKeysAnyFilter(rowKeys), cancellationToken: cancellationToken);

        var entities = new List<ITableEntityKey>();
        await foreach (var entity in entityQuery)
            entities.Add(new TableEntityKey(entity.PartitionKey, entity.RowKey));
        return entities;
    }

    public async Task<Stream> LoggingDownloadAsync(string blobName, CancellationToken cancellationToken)
    {
        var client = await this.clientFactory.GetAppendBlobClientAsync(BlobContainerNames.StationLogs, blobName, cancellationToken);
        return await client.OpenReadAsync(false, cancellationToken: cancellationToken);
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
            var client = await this.clientFactory.GetTableClientAsync(ItemTableNames.UserAssignedEntity(data), cancellationToken);
            var assignment = await client.GetEntityAsync<AzureUserAssignedEntitiesTableEntry>(
                userId, entityId, cancellationToken: cancellationToken);
            return assignment.Value != null;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }

    public async Task<Dictionary<string, ICollection<string>>> AssignedUsersAsync(
        TableEntityType type, 
        IEnumerable<string> entityIds,
        CancellationToken cancellationToken)
    {
        try
        {
            var assignedUsers = new Dictionary<string, ICollection<string>>();
            var entityIdsList = entityIds.ToList();
            if (!entityIdsList.Any()) 
                return assignedUsers;

            var client = await this.clientFactory.GetTableClientAsync(ItemTableNames.UserAssignedEntity(type), cancellationToken);
            var assigned = client.QueryAsync<AzureUserAssignedEntitiesTableEntry>(
                RowsWithKeysAnyFilter(entityIdsList), cancellationToken: cancellationToken);

            await foreach (var entity in assigned)
                if (assignedUsers.ContainsKey(entity.RowKey))
                    assignedUsers[entity.RowKey].Add(entity.PartitionKey);
                else assignedUsers[entity.RowKey] = new List<string> {entity.PartitionKey};

            return assignedUsers;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return new Dictionary<string, ICollection<string>>();
        }
    }

    private async Task<IEnumerable<IUserAssignedEntityTableEntry>> UserAssignedAsync(string userId, TableEntityType data, CancellationToken cancellationToken)
    {
        try
        {
            var client = await this.clientFactory.GetTableClientAsync(ItemTableNames.UserAssignedEntity(data), cancellationToken);
            var assigned = client.QueryAsync<AzureUserAssignedEntitiesTableEntry>(
                entry => entry.PartitionKey == userId,
                cancellationToken: cancellationToken);

            var assignedItems = new List<IUserAssignedEntityTableEntry>();
            await foreach (var entity in assigned)
                assignedItems.Add(new UserAssignedEntityTableEntry(userId, entity.RowKey));
            return assignedItems;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return Enumerable.Empty<IUserAssignedEntityTableEntry>();
        }
    }
}