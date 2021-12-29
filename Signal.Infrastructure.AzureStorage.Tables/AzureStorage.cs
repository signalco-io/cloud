using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Signal.Core;
using Signal.Core.Storage;
using ITableEntity = Signal.Core.Storage.ITableEntity;

namespace Signal.Infrastructure.AzureStorage.Tables;

internal class AzureStorage : IAzureStorage
{
    private readonly IAzureStorageClientFactory clientFactory;


    public AzureStorage(
        IAzureStorageClientFactory clientFactory)
    {
        this.clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
    }
        

    public async Task UpdateItemAsync<T>(string tableName, T item, CancellationToken cancellationToken) where T : ITableEntity
    {
        var client = await this.clientFactory.GetTableClientAsync(tableName, cancellationToken);
        var azureItem = new TableEntity(ObjectToDictionary(item)).EscapeKeys();
        await client.UpdateEntityAsync(azureItem, ETag.All, TableUpdateMode.Merge, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task CreateOrUpdateItemAsync<T>(string tableName, T item, CancellationToken cancellationToken) where T : ITableEntity
    {
        var client = await this.clientFactory.GetTableClientAsync(tableName, cancellationToken);
        var azureItem = new TableEntity(ObjectToDictionary(item)).EscapeKeys();
        await client.UpsertEntityAsync(azureItem, TableUpdateMode.Merge, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteItemAsync(
        string tableName, 
        string partitionKey, 
        string rowKey,
        CancellationToken cancellationToken)
    {
        var client = await this.clientFactory.GetTableClientAsync(tableName, cancellationToken);
        await client.DeleteEntityAsync(partitionKey, rowKey, cancellationToken: cancellationToken);
    }

    private static Dictionary<string, object?> ObjectToDictionary(object item) => 
        item.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(item));

    public async Task AppendToFileAsync(string directory, string fileName, Stream data, CancellationToken cancellationToken)
    {
        var client = await this.clientFactory.GetAppendBlobClientAsync(
            BlobContainerNames.StationLogs, 
            $"{directory.Replace("\\", "/")}/{fileName}",
            cancellationToken);

        // TODO: Handle data sizes over 4MB
        await client.AppendBlockAsync(data, cancellationToken: cancellationToken);
    }
}