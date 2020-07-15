using System.Threading.Tasks;

namespace Signal.Core
{
    public interface IAzureStorage
    {
        Task CreateTableAsync(string name);
        Task<AzureStorageQueuesList> ListQueues();
        Task<AzureStorageTablesList> ListTables();
    }
}
