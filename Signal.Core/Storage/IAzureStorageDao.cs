using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Signal.Core.Beacon;
using Signal.Core.Dashboards;
using Signal.Core.Devices;
using Signal.Core.Processes;
using Signal.Core.Users;

namespace Signal.Core.Storage
{
    public interface IAzureStorageDao
    {
        public Task<IEnumerable<IDeviceStateTableEntity>> GetDeviceStatesAsync(
            IEnumerable<string> deviceIds,
            CancellationToken cancellationToken);

        Task<bool> IsUserAssignedAsync(string userId, TableEntityType type, string entityId, CancellationToken cancellationToken);

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

        Task<string?> UserIdByEmailAsync(string userEmail, CancellationToken cancellationToken);

        public Task<Dictionary<string, ICollection<string>>> AssignedUsersAsync(
            TableEntityType type,
            IEnumerable<string> entityIds,
            CancellationToken cancellationToken);

        Task<IEnumerable<IBeaconTableEntity>> BeaconsAsync(string userId, CancellationToken cancellationToken);

        Task<IEnumerable<ITableEntityKey>> EntitiesByRowKeysAsync(
            string tableName,
            IEnumerable<string> rowKeys,
            CancellationToken cancellationToken);

        IAsyncEnumerable<IBlobInfo> LoggingListAsync(string stationId, CancellationToken cancellationToken);

        Task<Stream> LoggingDownloadAsync(string blobName, CancellationToken cancellationToken);
    }
}