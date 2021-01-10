using System;
using System.Collections.Generic;
using System.Linq;
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

        private async Task<IEnumerable<IDeviceTableEntity>> DevicesAsync(string userId, CancellationToken cancellationToken)
        {
            // Retrieve user assigned devices
            var userAssignedEntities = await this.UserAsync(userId, UserData.AssignedEntities, cancellationToken);
            if (userAssignedEntities == null)
                return Enumerable.Empty<IDeviceTableEntity>();

            // Split assigned device ids
            var assignedDeviceIds = userAssignedEntities.Devices.ToList();

            // Query user assigned devices
            var client = await this.GetTableClientAsync(ItemTableNames.Devices, cancellationToken).ConfigureAwait(false);
            var devicesQuery = client.QueryAsync<AzureDeviceTableEntity>(
                tableEntity => tableEntity.PartitionKey == userId && assignedDeviceIds.Contains(tableEntity.RowKey),
                cancellationToken: cancellationToken);
            
            // Retrieve and map devices
            var devices = new List<IDeviceTableEntity>();
            await foreach (var device in devicesQuery)
                devices.Add(new DeviceTableEntity(device.RowKey, device.DeviceIdentifier, device.Alias));
            return devices;
        }

        public async Task<IUserAssignedEntitiesTableEntry?> UserAsync(string userId, UserData data, CancellationToken cancellationToken)
        {
            var client = await this.GetTableClientAsync(ItemTableNames.Users, cancellationToken).ConfigureAwait(false);
            try
            {
                var user = await client.GetEntityAsync<AzureUserAssignedEntitiesTableEntry>(
                    userId, data.ToString(), cancellationToken: cancellationToken);
                return new UserAssignedEntitiesTableEntry(userId, data,
                    user.Value.Devices.Split(",", StringSplitOptions.RemoveEmptyEntries));
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
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