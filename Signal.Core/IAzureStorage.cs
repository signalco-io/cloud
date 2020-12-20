using System;
using System.Threading;
using System.Threading.Tasks;

namespace Signal.Core
{
    public interface IAzureStorage
    {
        Task QueueMessageAsync<T>(string queueName, T item, CancellationToken cancellationToken, TimeSpan? delay = null, TimeSpan? ttl = null)
            where T : class, IQueueItem;

        Task CreateItemAsync<T>(string tableName, T beaconItem, CancellationToken cancellationToken)
            where T : class, ITableEntity, new();
    }
}
