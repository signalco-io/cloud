using System;
using System.Threading;
using System.Threading.Tasks;

namespace Signal.Core
{
    public interface IAzureStorage
    {
        Task QueueItemAsync<T>(string queueName, T item, CancellationToken cancellationToken, TimeSpan? delay = null, TimeSpan? ttl = null)
            where T : class, IQueueItem;

        Task CreateOrUpdateItemAsync<T>(string tableName, T beaconItem, CancellationToken cancellationToken)
            where T : ITableEntity;

        Task EnsureTableExistsAsync(string tableName, CancellationToken cancellationToken);
    }
}
