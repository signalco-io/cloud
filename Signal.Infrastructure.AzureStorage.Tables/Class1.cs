using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Signal.Core;

namespace Signal.Infrastructure.AzureStorage.Tables
{
    public class AzureStorage
    {
        public static async Task<IEnumerable<string>> ListTables()
        {
            var tableClient = await GetTableClientAsync();

            var tableContinuationToken = new TableContinuationToken();
            var tables = await tableClient.ListTablesSegmentedAsync(tableContinuationToken);
            return tables.Results.Select(t => t.Uri.ToString());
        }

        public static async Task<IEnumerable<string>> ListQueues()
        {
            var client = await GetQueueClientAsync();
            var queueContinuationToken = new QueueContinuationToken();
            var queues = await client.ListQueuesSegmentedAsync(queueContinuationToken);
            return queues.Results.Select(q => q.Uri.ToString());
        }

        public static async Task CreateTableAsync(string name)
        {
            var tableClient = await GetTableClientAsync();
            var table = tableClient.GetTableReference(name);
            await table.CreateIfNotExistsAsync();
        }

        private static async Task<CloudQueueClient> GetQueueClientAsync()
        {
            var storageAccount = await GetStorageAccountAsync();
            return storageAccount.CreateCloudQueueClient();
        }

        private static async Task<CloudTableClient> GetTableClientAsync()
        {
            var storageAccount = await GetStorageAccountAsync();
            return storageAccount.CreateCloudTableClient();
        }

        private static async Task<CloudStorageAccount> GetStorageAccountAsync()
        {
            return CloudStorageAccount.Parse(await GetStorageConnectionString());
        }

        private static async Task<string> GetStorageConnectionString()
        {
            // TODO: Move to secret provider
            var client = new SecretClient(
                new Uri("https://signalapi.vault.azure.net/"), new DefaultAzureCredential());
            return (await client.GetSecretAsync(SecretKeys.StorageAccountConnectionString)).Value.Value;
        }
    }
}