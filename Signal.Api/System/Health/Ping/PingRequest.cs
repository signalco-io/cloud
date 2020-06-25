using Voyager.Api;

namespace Signal.Api.System.Health.Ping
{
    [Voyager.Api.Route(HttpMethod.Get, "system/health/ping")]
    public class PingRequest : EndpointRequest<PingResponse>
    {

    }
}
