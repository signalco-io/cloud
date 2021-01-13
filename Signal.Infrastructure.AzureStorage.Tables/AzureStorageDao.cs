using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Signal.Core;

namespace Signal.Infrastructure.AzureStorage.Tables
{
    internal class AzureStorageDao : IAzureStorageDao
    {
        private readonly ISecretsProvider secretsProvider;

        public AzureStorageDao(ISecretsProvider secretsProvider)
        {
            this.secretsProvider = secretsProvider ?? throw new ArgumentNullException(nameof(secretsProvider));
        }

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

        private static string PartitionWithKeysAnyFilter(string partitionKey, IEnumerable<string> rowKeys)
        {
            return $"(PartitionKey eq '{partitionKey}') and" +
                   $"({string.Join(" or", rowKeys.Select(tl => $"(RowKey eq '{tl}')"))})";
        }

        public async Task<IEnumerable<IDeviceTableEntity>> DevicesAsync(string userId, CancellationToken cancellationToken)
        {
            // Retrieve user assigned devices
            var userAssignedDevices = await this.UserAssignedAsync(userId, EntityType.Device, cancellationToken);
            
            // Split assigned device ids
            var assignedDeviceIds = userAssignedDevices.Select(d => d.RowKey).ToList();
            if (!assignedDeviceIds.Any())
                return Enumerable.Empty<IDeviceTableEntity>();

            // Query user assigned devices
            var client = await this.GetTableClientAsync(ItemTableNames.Devices, cancellationToken).ConfigureAwait(false);
            var devicesQuery = client.QueryAsync<AzureDeviceTableEntity>(
                PartitionWithKeysAnyFilter("device", assignedDeviceIds),
                cancellationToken: cancellationToken);
            
            // Retrieve and map devices
            var devices = new List<IDeviceTableEntity>();
            await foreach (var device in devicesQuery)
                devices.Add(new DeviceTableEntity(device.RowKey, device.DeviceIdentifier, device.Alias));
            return devices;
        }

        public async Task<bool> IsUserAssignedAsync(string userId, EntityType data, string entityId, CancellationToken cancellationToken)
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

        public async Task<IEnumerable<IUserAssignedEntityTableEntry>> UserAssignedAsync(string userId, EntityType data, CancellationToken cancellationToken)
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