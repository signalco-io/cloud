using System.Collections.Generic;
using Signal.Api.Dtos;

namespace Signal.Api.System.Storage.Queues.List
{
    public class StorageQueuesListResponse : ListResponse<string>
    {
        public StorageQueuesListResponse(IEnumerable<string> items) : base(items)
        {
        }
    }
}
