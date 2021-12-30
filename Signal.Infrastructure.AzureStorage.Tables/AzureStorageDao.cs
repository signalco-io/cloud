using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Signal.Core;
using Signal.Core.Beacon;
using Signal.Core.Dashboards;
using Signal.Core.Devices;
using Signal.Core.Processes;
using Signal.Core.Storage;
using Signal.Core.Users;
using ITableEntity = Azure.Data.Tables.ITableEntity;

namespace Signal.Infrastructure.AzureStorage.Tables;

internal class AzureStorageDao : IAzureStorageDao
{
    private readonly IAzureStorageClientFactory clientFactory;


    public AzureStorageDao(IAzureStorageClientFactory clientFactory)
    {
        this.clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
    }


    public async Task<IEnumerable<IDeviceStateHistoryTableEntity>> GetDeviceStateHistoryAsync(
        string deviceId,
        string channelName,
        string contactName,
        TimeSpan duration,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = await this.clientFactory.GetTableClientAsync(ItemTableNames.DevicesStatesHistory, cancellationToken)
                .ConfigureAwait(false);
            var history = client.QueryAsync<AzureDeviceStateHistoryTableEntity>(entry =>
                entry.PartitionKey == $"{deviceId}-{channelName}-{contactName}");

            // Limit to 30 days
            // TODO: Move this check to BLL
            var correctedDuration = duration;
            if (correctedDuration > TimeSpan.FromDays(30))
                correctedDuration = TimeSpan.FromDays(30);

            // Fetch all until reaching requested duration
            var items = new List<IDeviceStateHistoryTableEntity>();
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
            return Enumerable.Empty<IDeviceStateHistoryTableEntity>();
        }
    }

    public async Task<string?> UserIdByEmailAsync(string userEmail, CancellationToken cancellationToken)
    {
        try
        {
            var client = await this.clientFactory.GetTableClientAsync(ItemTableNames.Users, cancellationToken)
                .ConfigureAwait(false);
            var query = client.QueryAsync<AzureUserTableEntity>(u => u.Email == userEmail);
            await foreach (var match in query)
                return match.RowKey;
            return null;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<IUserTableEntity?> UserAsync(string userId, CancellationToken cancellationToken)
    {
        try
        {
            var client = await this.clientFactory.GetTableClientAsync(ItemTableNames.Users, cancellationToken)
                .ConfigureAwait(false);
            return (await client.GetEntityAsync<AzureUserTableEntity>(
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
        

    public async Task<IEnumerable<IDeviceStateTableEntity>> GetDeviceStatesAsync(
        IEnumerable<string> deviceIds,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = await this.clientFactory.GetTableClientAsync(ItemTableNames.DeviceStates, cancellationToken)
                .ConfigureAwait(false);
            var statesAsync = client.QueryAsync<AzureDeviceStateTableEntity>(PartitionsAnyFilter(deviceIds));
            var states = new List<IDeviceStateTableEntity>();
            await foreach (var state in statesAsync)
                if (state != null)
                    states.Add(state);
            return states;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return Enumerable.Empty<IDeviceStateTableEntity>();
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
        TableEntityType type,
        string tableName,
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
        var client = await this.clientFactory.GetTableClientAsync(tableName, cancellationToken).ConfigureAwait(false);
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
        var client = await this.clientFactory.GetTableClientAsync(tableName, cancellationToken).ConfigureAwait(false);
        var entityQuery = client.QueryAsync<TableEntity>(RowsWithKeysAnyFilter(rowKeys), cancellationToken: cancellationToken);

        var entities = new List<ITableEntityKey>();
        await foreach (var entity in entityQuery)
            entities.Add(new TableEntityKey(entity.PartitionKey, entity.RowKey));
        return entities;
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
            var info = new BlobInfo
            {
                Name = blobHierarchyItem.Blob.Name,
                CreatedTimeStamp = blobHierarchyItem.Blob.Properties.CreatedOn,
                LastModifiedTimeStamp = blobHierarchyItem.Blob.Properties.LastModified,
                Size = blobHierarchyItem.Blob.Properties.ContentLength
            };

            yield return info;
        }
    }

    public async Task<IEnumerable<IProcessTableEntity>> ProcessesAsync(
        string userId,
        CancellationToken cancellationToken) =>
        await this.GetUserAssignedAsync<IProcessTableEntity, AzureProcessTableEntity>(
            userId,
            TableEntityType.Process,
            ItemTableNames.Processes,
            null,
            process => new ProcessTableEntity(
                process.PartitionKey, 
                process.RowKey,
                process.Alias, 
                process.IsDisabled, 
                process.ConfigurationSerialized),
            cancellationToken);

    public async Task<IEnumerable<IDeviceTableEntity>> DevicesAsync(string userId,
        CancellationToken cancellationToken) =>
        await this.GetUserAssignedAsync<IDeviceTableEntity, AzureDeviceTableEntity>(
            userId,
            TableEntityType.Device,
            ItemTableNames.Devices,
            "device",
            device => new DeviceTableEntity(
                device.RowKey, device.DeviceIdentifier, device.Alias,
                device.Manufacturer, device.Model, device.Endpoints),
            cancellationToken);

    public async Task<bool> IsUserAssignedAsync(string userId, TableEntityType data, string entityId, CancellationToken cancellationToken)
    {
        try
        {
            var client = await this.clientFactory.GetTableClientAsync(ItemTableNames.UserAssignedEntity(data), cancellationToken)
                .ConfigureAwait(false);
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
            var client = await this.clientFactory.GetTableClientAsync(ItemTableNames.UserAssignedEntity(type), cancellationToken).ConfigureAwait(false);
            var assigned = client.QueryAsync<AzureUserAssignedEntitiesTableEntry>(
                RowsWithKeysAnyFilter(entityIds), cancellationToken: cancellationToken);

            var assignedUsers = new Dictionary<string, ICollection<string>>();
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

    public async Task<IEnumerable<IBeaconTableEntity>> BeaconsAsync(string userId, CancellationToken cancellationToken) =>
        await this.GetUserAssignedAsync<IBeaconTableEntity, AzureBeaconTableEntity>(
            userId,
            TableEntityType.Station,
            ItemTableNames.Beacons,
            null,
            beacon => new BeaconTableEntity(beacon.PartitionKey, beacon.RowKey)
            {
                RegisteredTimeStamp = beacon.RegisteredTimeStamp,
                Version = beacon.Version,
                StateTimeStamp = beacon.StateTimeStamp,
                AvailableWorkerServices = beacon.AvailableWorkerServices != null ? JsonSerializer.Deserialize<IEnumerable<string>>(beacon.AvailableWorkerServices) ?? Enumerable.Empty<string>() : Enumerable.Empty<string>(),
                RunningWorkerServices = beacon.RunningWorkerServices != null ? JsonSerializer.Deserialize<IEnumerable<string>>(beacon.RunningWorkerServices) ?? Enumerable.Empty<string>() : Enumerable.Empty<string>()
            },
            cancellationToken);

    private async Task<IEnumerable<IUserAssignedEntityTableEntry>> UserAssignedAsync(string userId, TableEntityType data, CancellationToken cancellationToken)
    {
        try
        {
            var client = await this.clientFactory.GetTableClientAsync(ItemTableNames.UserAssignedEntity(data), cancellationToken).ConfigureAwait(false);
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