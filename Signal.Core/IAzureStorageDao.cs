using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Signal.Core
{
    public interface IAzureStorageDao
    {
        Task<IDeviceStateTableEntity?> GetDeviceStateAsync(ITableEntityKey key, CancellationToken cancellationToken);

        Task<bool> DeviceExistsAsync(string deviceId, CancellationToken cancellationToken);

        Task<string?> DeviceExistsAsync(string userId, string deviceIdentifier, CancellationToken cancellationToken);

        Task<bool> IsUserAssignedAsync(string userId, EntityType type, string entityId, CancellationToken cancellationToken);

        Task<IEnumerable<IUserAssignedEntityTableEntry>> UserAssignedAsync(string userId, EntityType type, CancellationToken cancellationToken);

        Task<IEnumerable<IDeviceTableEntity>> DevicesAsync(string userId, CancellationToken cancellationToken);
    }
}