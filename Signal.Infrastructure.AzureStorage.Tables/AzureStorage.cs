using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Signal.Core;

namespace Signal.Infrastructure.AzureStorage.Tables
{
    public class AzureStorage : IAzureStorage
    {
        private readonly ISecretsProvider secretsProvider;

        public AzureStorage(ISecretsProvider secretsProvider)
        {
            this.secretsProvider = secretsProvider ?? throw new ArgumentNullException(nameof(secretsProvider));
        }
        
        public async Task QueueMessageAsync<T>(string queueName, T item, CancellationToken cancellationToken, TimeSpan? delay = null, TimeSpan? ttl = null)
            where T : class, IQueueItem
        {
            var itemSerialized = JsonSerializer.Serialize(item);
            var itemSerializedBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(itemSerialized));
            var client = await this.GetQueueClientAsync(queueName, cancellationToken).ConfigureAwait(false);
            await client.SendMessageAsync(BinaryData.FromString(itemSerializedBase64), delay, ttl, cancellationToken);
        }

        public async Task CreateItemAsync<T>(string tableName, T item, CancellationToken cancellationToken) where T : class, Core.ITableEntity, new()
        {
            var client = await this.GetTableClientAsync(tableName, cancellationToken);
            var azureItem = new TableEntity(ObjectToDictionary(item));
            await client.AddEntityAsync(azureItem, cancellationToken);
        }

        private static Dictionary<string, object> ObjectToDictionary<T>(T item) where T : class, Core.ITableEntity, new() => 
            item.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(item));

        public async Task<TableClient> GetTableClientAsync(string tableName, CancellationToken cancellationToken)
        {
            var connectionString = await this.GetConnectionStringAsync(cancellationToken);
            return new TableClient(connectionString, tableName);
        }

        private async Task<QueueClient> GetQueueClientAsync(string queueName, CancellationToken cancellationToken)
        {
            var connectionString = await this.GetConnectionStringAsync(cancellationToken);
            return new QueueClient(connectionString, queueName);
        }

        private async Task<string> GetConnectionStringAsync(CancellationToken cancellationToken)
        {
            return await this.secretsProvider.GetSecretAsync(SecretKeys.StorageAccountConnectionString, cancellationToken).ConfigureAwait(false);
        }
    }
}