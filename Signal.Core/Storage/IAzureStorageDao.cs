using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Signal.Core.Dashboards;
using Signal.Core.Devices;
using Signal.Core.Processes;

namespace Signal.Core.Storage
{
    public interface IAzureStorageDao
    {
        public Task<IEnumerable<IDeviceStateTableEntity>> GetDeviceStatesAsync(
            IEnumerable<string> deviceIds,
            CancellationToken cancellationToken);

        Task<IDeviceStateTableEntity?> GetDeviceStateAsync(ITableEntityKey key, CancellationToken cancellationToken);

        Task<bool> DeviceExistsAsync(string deviceId, CancellationToken cancellationToken);

        Task<string?> DeviceExistsAsync(string userId, string deviceIdentifier, CancellationToken cancellationToken);

        Task<bool> IsUserAssignedAsync(string userId, TableEntityType type, string entityId, CancellationToken cancellationToken);

        Task<IEnumerable<IUserAssignedEntityTableEntry>> UserAssignedAsync(string userId, TableEntityType type, CancellationToken cancellationToken);

        Task<IEnumerable<IDeviceTableEntity>> DevicesAsync(string userId, CancellationToken cancellationToken);

        Task<IEnumerable<IProcessTableEntity>> ProcessesAsync(string userId, CancellationToken cancellationToken);

        Task<IEnumerable<IDeviceStateHistoryTableEntity>> GetDeviceStateHistoryAsync(
            string deviceId,
            string channelName,
            string contactName,
            TimeSpan duration,
            CancellationToken cancellationToken);

        Task<IEnumerable<IDashboardTableEntity>> DashboardsAsync(string userId, CancellationToken cancellationToken);

        Task<IUserTableEntity?> UserAsync(string userId, CancellationToken cancellationToken);
    }
}