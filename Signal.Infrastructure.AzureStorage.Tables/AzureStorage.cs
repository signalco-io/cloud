using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Signal.Core;
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
        
        public async Task<IDeviceStateTableEntity> GetDeviceStateAsync(ITableEntityKey key, CancellationToken cancellationToken)
        {
            var client = await this.GetTableClientAsync("devicestates", cancellationToken).ConfigureAwait(false);
            var response = await client.GetEntityAsync<AzureDeviceStateTableEntity>(key.PartitionKey, key.RowKey, cancellationToken: cancellationToken).ConfigureAwait(false);
            return response.Value;
        }
        
        // TODO: De-dup AzureStorage
        private async Task<TableClient> GetTableClientAsync(string tableName, CancellationToken cancellationToken) => 
            new TableClient(await this.GetConnectionStringAsync(cancellationToken).ConfigureAwait(false), tableName);

        // TODO: De-dup AzureStorage
        private async Task<string> GetConnectionStringAsync(CancellationToken cancellationToken) =>
            await this.secretsProvider.GetSecretAsync(SecretKeys.StorageAccountConnectionString, cancellationToken).ConfigureAwait(false);
    }

    internal class AzureTableEntityBase : ITableEntity
    {
        public string PartitionKey { get; set; }
        
        public string RowKey { get; set; }
        
        public DateTimeOffset? Timestamp { get; set; }
        
        public ETag ETag { get; set; }
    }
    
    internal class AzureDeviceStateTableEntity : AzureTableEntityBase, IDeviceStateTableEntity
    {
        public string DeviceIdentifier { get; set; }
        
        public string ChannelName { get; set; }
        
        public string ContactName { get; set; }
        
        public string? ValueSerialized { get; set; }
    }
    
    internal class AzureStorage : IAzureStorage
    {
        private readonly ISecretsProvider secretsProvider;

        public AzureStorage(ISecretsProvider secretsProvider)
        {
            this.secretsProvider = secretsProvider ?? throw new ArgumentNullException(nameof(secretsProvider));
        }
        
        public async Task QueueItemAsync<T>(string queueName, T item, CancellationToken cancellationToken, TimeSpan? delay = null, TimeSpan? ttl = null)
            where T : class, IQueueItem
        {
            var itemSerialized = JsonSerializer.Serialize(item);
            var itemSerializedBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(itemSerialized));
            var client = await this.GetQueueClientAsync(queueName, cancellationToken).ConfigureAwait(false);
            await client.SendMessageAsync(BinaryData.FromString(itemSerializedBase64), delay, ttl, cancellationToken).ConfigureAwait(false);
        }

        public async Task CreateOrUpdateItemAsync<T>(string tableName, T item, CancellationToken cancellationToken) where T : Core.ITableEntity
        {
            var client = await this.GetTableClientAsync(tableName, cancellationToken);
            var azureItem = new TableEntity(ObjectToDictionary(item));
            await client.UpsertEntityAsync(azureItem, TableUpdateMode.Merge, cancellationToken).ConfigureAwait(false);
        }

        private static Dictionary<string, object> ObjectToDictionary(object item) => 
            item.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(item));

        private async Task<TableClient> GetTableClientAsync(string tableName, CancellationToken cancellationToken) => 
            new TableClient(await this.GetConnectionStringAsync(cancellationToken).ConfigureAwait(false), tableName);

        private async Task<QueueClient> GetQueueClientAsync(string queueName, CancellationToken cancellationToken) => 
            new QueueClient(await this.GetConnectionStringAsync(cancellationToken).ConfigureAwait(false), queueName);

        private async Task<string> GetConnectionStringAsync(CancellationToken cancellationToken) =>
            await this.secretsProvider.GetSecretAsync(SecretKeys.StorageAccountConnectionString, cancellationToken).ConfigureAwait(false);
    }
}
