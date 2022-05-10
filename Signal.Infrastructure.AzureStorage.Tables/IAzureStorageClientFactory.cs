using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;

namespace Signal.Infrastructure.AzureStorage.Tables;

public interface IAzureStorageClientFactory
{
    Task<TableClient> GetTableClientAsync(string tableName, CancellationToken cancellationToken = default);
    Task<AppendBlobClient> GetAppendBlobClientAsync(string containerName, string filePath, CancellationToken cancellationToken);
    Task<BlobContainerClient> GetBlobContainerClientAsync(string containerName, CancellationToken cancellationToken);
}