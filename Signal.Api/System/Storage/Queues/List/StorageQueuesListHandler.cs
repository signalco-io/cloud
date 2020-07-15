using System;
using Signal.Api.Handlers;
using Signal.Core;

namespace Signal.Api.System.Storage.Queues.List
{

    public class StorageQueuesListHandler : ServiceHandler<StorageQueuesListRequest, StorageQueuesListResponse, IAzureStorage, AzureStorageQueuesList>
    {
        public StorageQueuesListHandler(IServiceProvider serviceProvider)
            : base(serviceProvider, (req, service) => service.ListQueues())
        {
        }
    }
}
