﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Signal.Core;

namespace Signal.Infrastructure.AzureStorage.Tables;

public class AzureStorageClientFactory : IAzureStorageClientFactory
{
    private static readonly ConcurrentDictionary<string, BlobContainerClient> EstablishedBlobContainerClients = new();
    private static readonly ConcurrentDictionary<string, AppendBlobClient> EstablishedAppendBlobClients = new();
    private static readonly ConcurrentDictionary<string, TableClient> EstablishedTableClients = new();
    private readonly ISecretsProvider secretsProvider;
    

    public AzureStorageClientFactory(
        ISecretsProvider secretsProvider)
    {
        this.secretsProvider = secretsProvider ?? throw new ArgumentNullException(nameof(secretsProvider));
    }

    public async Task<BlobContainerClient> GetBlobContainerClientAsync(string containerName, CancellationToken cancellationToken)
    {
        // Return established client if available
        if (EstablishedBlobContainerClients.TryGetValue(containerName, out var client))
            return client;
        
        client = new BlobContainerClient(
            await GetConnectionStringAsync(cancellationToken), 
            containerName);
        EstablishedBlobContainerClients.TryAdd(containerName, client);

        // Create container if doesn't exist
        await client.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        return client;
    }

    public async Task<AppendBlobClient> GetAppendBlobClientAsync(string containerName, string filePath, CancellationToken cancellationToken)
    {
        // Return established client if available
        var cacheKey = $"{containerName}|{filePath}";
        if (EstablishedAppendBlobClients.TryGetValue(cacheKey, out var client))
            return client;

        var container = await this.GetBlobContainerClientAsync(containerName, cancellationToken);
        client = container.GetAppendBlobClient(filePath);
        EstablishedAppendBlobClients.TryAdd(cacheKey, client);

        // Create append blob if doesn't exist
        await client.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        return client;
    }

    public async Task<TableClient> GetTableClientAsync(string tableName, CancellationToken cancellationToken = default)
    {
        // Return established client if available
        if (EstablishedTableClients.TryGetValue(tableName, out var client))
            return client;

        // Instantiate new client and persist
        client = new TableClient(
            await this.GetConnectionStringAsync(cancellationToken),
            AzureTableExtensions.EscapeKey(tableName));
        EstablishedTableClients.TryAdd(tableName, client);

        return client;
    }

    private async Task<string> GetConnectionStringAsync(CancellationToken cancellationToken = default) =>
        await this.secretsProvider.GetSecretAsync(SecretKeys.StorageAccountConnectionString, cancellationToken);
}