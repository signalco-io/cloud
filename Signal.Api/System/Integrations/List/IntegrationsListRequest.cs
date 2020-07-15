using Voyager.Api;

namespace Signal.Api.System.Integrations.List
{

    [Voyager.Api.Route(HttpMethod.Get, "system/integrations/list")]
    public class IntegrationsListRequest : EndpointRequest<IntegrationsListResponse>
    {
    }
}
