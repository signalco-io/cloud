using System.Threading;
using System.Threading.Tasks;

namespace Signal.Core
{
    public interface IAzureStorage
    {
        Task CreateTableAsync(string name, CancellationToken cancellationToken);
        Task<AzureStorageQueuesList> ListQueues(CancellationToken cancellationToken);
        Task<AzureStorageTablesList> ListTables(CancellationToken cancellationToken);
    }
}
