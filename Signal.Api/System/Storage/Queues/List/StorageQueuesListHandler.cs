using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Signal.Core;
using Voyager.Api;

namespace Signal.Api.System.Storage.Queues.List
{
    public class StorageQueuesListHandler : EndpointHandler<StorageQueuesListRequest, StorageQueuesListResponse>
    {
        private readonly IAzureStorage azureStorage;

        public StorageQueuesListHandler(IAzureStorage azureStorage)
        {
            this.azureStorage = azureStorage ?? throw new ArgumentNullException(nameof(azureStorage));
        }

        public override async Task<ActionResult<StorageQueuesListResponse>> HandleRequestAsync(StorageQueuesListRequest request)
        {
            var items = await this.azureStorage.ListQueues();
            return new StorageQueuesListResponse()
            {
                Items = items
            };
        }
    }
}
