using System;
using System.Linq;
using System.Threading;
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

        public async Task<AzureStorageTablesList> ListTables(CancellationToken cancellationToken)
        {
            var tableClient = await this.GetTableClientAsync(cancellationToken);

            var tableContinuationToken = new TableContinuationToken();
            var tables = await tableClient.ListTablesSegmentedAsync(tableContinuationToken);
            return new AzureStorageTablesList(tables.Results.Select(t => t.Uri.ToString()));
        }

        public async Task<AzureStorageQueuesList> ListQueues(CancellationToken cancellationToken)
        {
            var client = await this.GetQueueClientAsync(cancellationToken);
            var queueContinuationToken = new QueueContinuationToken();
            var queues = await client.ListQueuesSegmentedAsync(queueContinuationToken);
            return new AzureStorageQueuesList(queues.Results.Select(q => q.Uri.ToString()));
        }

        public async Task CreateTableAsync(string name, CancellationToken cancellationToken)
        {
            var tableClient = await this.GetTableClientAsync(cancellationToken);
            var table = tableClient.GetTableReference(name);
            await table.CreateIfNotExistsAsync();
        }

        private async Task<CloudQueueClient> GetQueueClientAsync(CancellationToken cancellationToken)
        {
            var storageAccount = await this.GetStorageAccountAsync(cancellationToken);
            return storageAccount.CreateCloudQueueClient();
        }

        private async Task<CloudTableClient> GetTableClientAsync(CancellationToken cancellationToken)
        {
            var storageAccount = await this.GetStorageAccountAsync(cancellationToken);
            return storageAccount.CreateCloudTableClient();
        }

        private async Task<CloudStorageAccount> GetStorageAccountAsync(CancellationToken cancellationToken) => 
            CloudStorageAccount.Parse(await this.GetStorageConnectionString(cancellationToken));

        private Task<string> GetStorageConnectionString(CancellationToken cancellationToken) =>
            this.secretsProvider.GetSecretAsync(SecretKeys.StorageAccountConnectionString, cancellationToken);
    }
}