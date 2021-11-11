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
using Signal.Core.Storage;
using ITableEntity = Signal.Core.Storage.ITableEntity;

namespace Signal.Infrastructure.AzureStorage.Tables
{
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

        public async Task UpdateItemAsync<T>(string tableName, T item, CancellationToken cancellationToken) where T : ITableEntity
        {
            var client = await this.GetTableClientAsync(tableName, cancellationToken);
            var azureItem = new TableEntity(ObjectToDictionary(item)).EscapeKeys();
            await client.UpdateEntityAsync(azureItem, ETag.All, TableUpdateMode.Merge, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task CreateOrUpdateItemAsync<T>(string tableName, T item, CancellationToken cancellationToken) where T : ITableEntity
        {
            var client = await this.GetTableClientAsync(tableName, cancellationToken);
            var azureItem = new TableEntity(ObjectToDictionary(item)).EscapeKeys();
            await client.UpsertEntityAsync(azureItem, TableUpdateMode.Merge, cancellationToken).ConfigureAwait(false);
        }

        public async Task EnsureTableExistsAsync(string tableName, CancellationToken cancellationToken)
        {
            var client = await this.GetTableClientAsync(tableName, cancellationToken);
            await client.CreateIfNotExistsAsync(cancellationToken);
        }

        public Task AppendToFileAsync(string directory, string fileName, string data, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private static Dictionary<string, object> ObjectToDictionary(object item) => 
            item.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(item));

        private async Task<TableClient> GetTableClientAsync(string tableName, CancellationToken cancellationToken) => 
            new TableClient(await this.GetConnectionStringAsync(cancellationToken).ConfigureAwait(false), AzureTableExtensions.EscapeTableName(tableName));

        private async Task<QueueClient> GetQueueClientAsync(string queueName, CancellationToken cancellationToken) => 
            new QueueClient(await this.GetConnectionStringAsync(cancellationToken).ConfigureAwait(false), queueName);

        private async Task<string> GetConnectionStringAsync(CancellationToken cancellationToken) =>
            await this.secretsProvider.GetSecretAsync(SecretKeys.StorageAccountConnectionString, cancellationToken).ConfigureAwait(false);
    }
}
