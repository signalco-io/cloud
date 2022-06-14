using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Signal.Core.Storage;

public interface IAzureStorage
{
    Task UpdateItemAsync<T>(string tableName, T item, CancellationToken cancellationToken)
        where T : ITableEntity;

    Task CreateOrUpdateItemAsync<T>(string tableName, T item, CancellationToken cancellationToken)
        where T : ITableEntity;

    Task DeleteItemAsync(
        string tableName, 
        string partitionKey, 
        string rowKey,
        CancellationToken cancellationToken);

    Task AppendToFileAsync(string directory, string fileName, Stream data, CancellationToken cancellationToken);
}