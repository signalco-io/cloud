using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Signal.Core;

namespace Signal.Infrastructure.AzureStorage.Tables;

public class AzureStorageClientFactory : IAzureStorageClientFactory
{
    private static readonly ConcurrentDictionary<string, TableClient> EstablishedClients = new();
    private readonly ISecretsProvider secretsProvider;
    

    public AzureStorageClientFactory(
        ISecretsProvider secretsProvider)
    {
        this.secretsProvider = secretsProvider ?? throw new ArgumentNullException(nameof(secretsProvider));
    }


    public async Task<TableClient> GetTableClientAsync(string tableName, CancellationToken cancellationToken)
    {
        // Return established client if available
        if (EstablishedClients.TryGetValue(tableName, out var client))
            return client;

        // Instantiate new client and persist
        client = new TableClient(
            await this.GetConnectionStringAsync(cancellationToken).ConfigureAwait(false),
            AzureTableExtensions.EscapeKey(tableName));
        EstablishedClients.TryAdd(tableName, client);

        return client;
    }

    private async Task<string> GetConnectionStringAsync(CancellationToken cancellationToken) =>
        await this.secretsProvider.GetSecretAsync(SecretKeys.StorageAccountConnectionString, cancellationToken).ConfigureAwait(false);
}