using System;
using System.Threading;
using System.Threading.Tasks;
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
}