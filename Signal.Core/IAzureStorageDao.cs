using System.Threading;
using System.Threading.Tasks;

namespace Signal.Core
{
    public interface IAzureStorageDao
    {
        Task<IDeviceStateTableEntity?> GetDeviceStateAsync(ITableEntityKey key, CancellationToken cancellationToken);

        Task<bool> DeviceExistsAsync(string deviceId, CancellationToken cancellationToken);

        Task<string?> DeviceExistsAsync(string userId, string deviceIdentifier, CancellationToken cancellationToken);
        Task<IUserAssignedEntitiesTableEntry?> UserAsync(string userId, UserData data, CancellationToken cancellationToken);
    }
}