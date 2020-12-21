using System.Threading;
using System.Threading.Tasks;

namespace Signal.Core
{
    public interface IAzureStorageDao
    {
        Task<IDeviceStateTableEntity?> GetDeviceStateAsync(ITableEntityKey key, CancellationToken cancellationToken);
    }
}