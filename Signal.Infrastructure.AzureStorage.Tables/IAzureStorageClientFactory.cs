using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;

namespace Signal.Infrastructure.AzureStorage.Tables;

public interface IAzureStorageClientFactory
{
    Task<TableClient> GetTableClientAsync(string tableName, CancellationToken cancellationToken);
}