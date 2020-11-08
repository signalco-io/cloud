using Voyager.Api;

namespace Signal.Api.System.Health.Ping
{
    [VoyagerRoute(HttpMethod.Get, "system/health/ping")]
    public class PingRequest : EndpointRequest<PingResponse>
    {
    }
}
