using Voyager.Api;

namespace Signal.Api.System.Storage.Tables.List
{
    [Voyager.Api.Route(HttpMethod.Get, "system/storage/tables/list")]
    public class GetStorageTablesRequest : EndpointRequest<GetStorageTablesResponse>
    {
    }
}
