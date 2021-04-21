using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Signal.Infrastructure.AzureStorage.Tables
{
    internal class AzureStorageDao : IAzureStorageDao
    {
        private readonly ISecretsProvider secretsProvider;

        public AzureStorageDao(ISecretsProvider secretsProvider)
        {
            this.secretsProvider = secretsProvider ?? throw new ArgumentNullException(nameof(secretsProvider));
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
                var client = await this.GetTableClientAsync(ItemTableNames.DevicesStatesHistory, cancellationToken)
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
                var client = await this.GetTableClientAsync(ItemTableNames.Users, cancellationToken)
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
                var client = await this.GetTableClientAsync(ItemTableNames.Users, cancellationToken)
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
                    dashboard.ConfigurationSerialized),
                cancellationToken);
        

        public async Task<IEnumerable<IDeviceStateTableEntity>> GetDeviceStatesAsync(
            IEnumerable<string> deviceIds,
            CancellationToken cancellationToken)
        {
            try
            {
                var client = await this.GetTableClientAsync(ItemTableNames.DeviceStates, cancellationToken)
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

        public async Task<IDeviceStateTableEntity?> GetDeviceStateAsync(ITableEntityKey key, CancellationToken cancellationToken)
        {
            try
            {
                var client = await this.GetTableClientAsync(ItemTableNames.DeviceStates, cancellationToken).ConfigureAwait(false);
                var response = await client.GetEntityAsync<AzureDeviceStateTableEntity>(
                    AzureTableExtensions.EscapeKey(key.PartitionKey),
                    AzureTableExtensions.EscapeKey(key.RowKey), 
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                var item = response.Value;
                return item;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
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
            var client = await this.GetTableClientAsync(tableName, cancellationToken).ConfigureAwait(false);
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

        public async Task<IEnumerable<IDeviceTableEntity>> DevicesAsync(string userId, CancellationToken cancellationToken) =>
            await this.GetUserAssignedAsync<IDeviceTableEntity, AzureDeviceTableEntity>(
                userId,
                TableEntityType.Device,
                ItemTableNames.Devices,
                "device",
                device => new DeviceTableEntity(device.RowKey, device.DeviceIdentifier, device.Alias)
                {
                    Endpoints = device.Endpoints,
                    Manufacturer = device.Manufacturer,
                    Model = device.Model
                },
                cancellationToken);

        public async Task<bool> IsUserAssignedAsync(string userId, TableEntityType data, string entityId, CancellationToken cancellationToken)
        {
            try
            {
                var client = await this.GetTableClientAsync(ItemTableNames.UserAssignedEntity(data), cancellationToken)
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

        public async Task<IEnumerable<IUserAssignedEntityTableEntry>> UserAssignedAsync(string userId, TableEntityType data, CancellationToken cancellationToken)
        {
            try
            {
                var client = await this.GetTableClientAsync(ItemTableNames.UserAssignedEntity(data), cancellationToken).ConfigureAwait(false);
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

        public async Task<bool> DeviceExistsAsync(string deviceId, CancellationToken cancellationToken)
        {
            try
            {
                var client = await this.GetTableClientAsync(ItemTableNames.Devices, cancellationToken)
                    .ConfigureAwait(false);
                var entity = await client.GetEntityAsync<AzureDeviceTableEntity>(
                    "devices", deviceId, cancellationToken: cancellationToken);
                return entity.Value != null;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return false;
            }
        }

        public async Task<string?> DeviceExistsAsync(string userId, string deviceIdentifier, CancellationToken cancellationToken)
        {
            var devices = await this.DevicesAsync(userId, cancellationToken);
            return devices.FirstOrDefault(d => d.DeviceIdentifier == deviceIdentifier) is { } matchedDevice
                ? matchedDevice.RowKey
                : null;
        }

        // TODO: De-dup AzureStorage
        private async Task<TableClient> GetTableClientAsync(string tableName, CancellationToken cancellationToken) => 
            new TableClient(await this.GetConnectionStringAsync(cancellationToken).ConfigureAwait(false), AzureTableExtensions.EscapeKey(tableName));

        // TODO: De-dup AzureStorage
        private async Task<string> GetConnectionStringAsync(CancellationToken cancellationToken) =>
            await this.secretsProvider.GetSecretAsync(SecretKeys.StorageAccountConnectionString, cancellationToken).ConfigureAwait(false);
    }
}