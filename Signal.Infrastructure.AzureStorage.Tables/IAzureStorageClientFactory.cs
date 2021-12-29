using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Azure.Storage.Blobs.Specialized;

namespace Signal.Infrastructure.AzureStorage.Tables;

public interface IAzureStorageClientFactory
{
    Task<TableClient> GetTableClientAsync(string tableName, CancellationToken cancellationToken);
    Task<AppendBlobClient> GetAppendBlobClientAsync(string containerName, string filePath, CancellationToken cancellationToken);
}