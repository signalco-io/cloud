using Voyager.Api;

namespace Signal.Api.System.Storage.Tables.List
{
    [VoyagerRoute(HttpMethod.Get, "system/storage/tables/list")]
    public class GetStorageTablesRequest : EndpointRequest<GetStorageTablesResponse>
    {
    }
}
