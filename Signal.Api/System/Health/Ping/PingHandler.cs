using Microsoft.AspNetCore.Mvc;
using Voyager.Api;

namespace Signal.Api.System.Health.Ping
{
    public class PingHandler : EndpointHandler<PingRequest, PingResponse>
    {
        public override ActionResult<PingResponse> HandleRequest(PingRequest request)
        {
            return new PingResponse
            {
                Version = typeof(PingRequest).Assembly.GetName().Version?.ToString()
            };
        }
    }
}
