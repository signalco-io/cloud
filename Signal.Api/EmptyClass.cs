using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Voyager.Api;

namespace Signal.Api
{
    public class GetVoyagerInfoResponse
    {
        public string Message { get; set; }
    }

    [Voyager.Api.Route(HttpMethod.Get, "voyager/info")]
    public class GetVoyagerInfoRequest : EndpointRequest<GetVoyagerInfoResponse>
    {
    }

    public class GetVoyagerInfoHandler : EndpointHandler<GetVoyagerInfoRequest, GetVoyagerInfoResponse>
    {
        public override ActionResult<GetVoyagerInfoResponse> HandleRequest(GetVoyagerInfoRequest request)
        {
            return new GetVoyagerInfoResponse { Message = "Voyager is awesome!" };
        }
    }

    public class Routes
    {
        private readonly HttpRouter router;

        public Routes(HttpRouter router)
        {
            this.router = router;
        }

        [FunctionName(nameof(FallbackRoute))]
        public Task<IActionResult> FallbackRoute([HttpTrigger(AuthorizationLevel.Anonymous, "get", "put", "delete", "post", "head", "trace", "patch", "connect", "options", Route = "{*path}")] HttpRequest req)
        {
            return router.Route(req.HttpContext);
        }
    }
}
