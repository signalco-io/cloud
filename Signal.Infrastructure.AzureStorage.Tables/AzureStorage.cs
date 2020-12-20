using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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
            where T : IQueueItem
        {
            var itemSerialized = JsonSerializer.Serialize(item);
            var itemSerializedBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(itemSerialized));
            var client = await this.GetQueueClientAsync(queueName, cancellationToken).ConfigureAwait(false);
            await client.SendMessageAsync(BinaryData.FromString(itemSerializedBase64), delay, ttl, cancellationToken);
        }
        
        private async Task<QueueClient> GetQueueClientAsync(string queueName, CancellationToken cancellationToken)
        {
            var connectionString = await this.secretsProvider.GetSecretAsync(SecretKeys.StorageAccountConnectionString, cancellationToken).ConfigureAwait(false);
            return new QueueClient(connectionString, queueName);
        }
    }
}