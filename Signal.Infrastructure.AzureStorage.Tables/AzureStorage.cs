using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
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
            var keyEscaped = key.EscapeKeys();
            var client = await this.GetTableClientAsync("devicestates", cancellationToken).ConfigureAwait(false);
            var response = await client.GetEntityAsync<AzureDeviceStateTableEntity>(keyEscaped.PartitionKey, keyEscaped.RowKey, cancellationToken: cancellationToken).ConfigureAwait(false);
            var item = response.Value;
            item.UnEscapeKeys();
            return item;
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
            var azureItem = new TableEntity(ObjectToDictionary(item)).EscapeKeys();
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
    
    internal static class TableEntityExtensions
    {
        public static TableEntity EscapeKeys(this TableEntity entity)
        {
            entity.PartitionKey = EscapeKey(entity.PartitionKey);
            entity.RowKey = EscapeKey(entity.RowKey);
            return entity;
        }
        
        public static TableEntity UnEscapeKeys(this TableEntity entity)
        {
            entity.PartitionKey = UnEscapeKey(entity.PartitionKey);
            entity.RowKey = UnEscapeKey(entity.RowKey);
            return entity;
        }
        
        public static ITableEntityKey EscapeKeys(this ITableEntityKey entity)
        {
            entity.PartitionKey = EscapeKey(entity.PartitionKey);
            entity.RowKey = EscapeKey(entity.RowKey);
            return entity;
        }
        
        public static ITableEntityKey UnEscapeKeys(this ITableEntityKey entity)
        {
            entity.PartitionKey = UnEscapeKey(entity.PartitionKey);
            entity.RowKey = UnEscapeKey(entity.RowKey);
            return entity;
        }
        
        private static string EscapeKey(string key)
        {
            return key
                .Replace("/", "__bs__")
                .Replace("\\", "__fs__")
                .Replace("#", "__hash__")
                .Replace("?", "__q__")
                .Replace("\t", "__tab__")
                .Replace("\n", "__nl__")
                .Replace("\r", "__cr__");
        }
        
        private static string UnEscapeKey(string key)
        {
            return key
                .Replace("__bs__", "/")
                .Replace("__fs__", "\\")
                .Replace("__hash__", "#")
                .Replace("__q__", "?")
                .Replace("__tab__", "\t")
                .Replace("__nl__", "\n")
                .Replace("__cr__", "\r");
        }
    }
}
