using System.Collections.Generic;
using System.Threading.Tasks;

namespace Signal.Core
{
    public interface IAzureStorage
    {
        Task CreateTableAsync(string name);
        Task<IEnumerable<string>> ListQueues();
        Task<IEnumerable<string>> ListTables();
    }
}
