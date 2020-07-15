using Voyager.Api;

namespace Signal.Api.System.Integrations.List
{
    [Voyager.Api.Route(HttpMethod.Get, "system/storage/queues/list")]
    public class IntegrationsListRequest : EndpointRequest<IntegrationsListResponse>
    {
    }
}
