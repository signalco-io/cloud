using Voyager.Api;

namespace Signal.Api.System.Storage.Queues.List
{
    [VoyagerRoute(HttpMethod.Get, "system/storage/queues/list")]
    public class StorageQueuesListRequest : EndpointRequest<StorageQueuesListResponse>
    {
    }
}
