using AutoMapper;
using Signal.Core;

namespace Signal.Api.System.Storage.Queues.List
{
    public class StorageQueuesMapperProfile : Profile
    {
        public StorageQueuesMapperProfile()
        {
            this.CreateMap<AzureStorageQueuesList, StorageQueuesListResponse>();
        }
    }
}
