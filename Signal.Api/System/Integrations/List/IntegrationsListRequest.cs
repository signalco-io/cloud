using Voyager.Api;

namespace Signal.Api.System.Integrations.List
{
    [VoyagerRoute(HttpMethod.Get, "system/integrations/list")]
    public class IntegrationsListRequest : EndpointRequest<IntegrationsListResponse>
    {
    }
}
