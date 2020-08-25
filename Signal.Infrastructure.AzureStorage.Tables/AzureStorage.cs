using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
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

        public async Task<AzureStorageTablesList> ListTables()
        {
            var tableClient = await GetTableClientAsync();

            var tableContinuationToken = new TableContinuationToken();
            var tables = await tableClient.ListTablesSegmentedAsync(tableContinuationToken);
            return new AzureStorageTablesList(tables.Results.Select(t => t.Uri.ToString()));
        }

        public async Task<AzureStorageQueuesList> ListQueues()
        {
            var client = await GetQueueClientAsync();
            var queueContinuationToken = new QueueContinuationToken();
            var queues = await client.ListQueuesSegmentedAsync(queueContinuationToken);
            return new AzureStorageQueuesList(queues.Results.Select(q => q.Uri.ToString()));
        }

        public async Task CreateTableAsync(string name)
        {
            var tableClient = await GetTableClientAsync();
            var table = tableClient.GetTableReference(name);
            await table.CreateIfNotExistsAsync();
        }

        private async Task<CloudQueueClient> GetQueueClientAsync()
        {
            var storageAccount = await GetStorageAccountAsync();
            return storageAccount.CreateCloudQueueClient();
        }

        private async Task<CloudTableClient> GetTableClientAsync()
        {
            var storageAccount = await GetStorageAccountAsync();
            return storageAccount.CreateCloudTableClient();
        }

        private async Task<CloudStorageAccount> GetStorageAccountAsync()
        {
            return CloudStorageAccount.Parse(await GetStorageConnectionString());
        }

        private Task<string> GetStorageConnectionString() =>
            this.secretsProvider.GetSecretAsync(SecretKeys.StorageAccountConnectionString);
    }
}