using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Voyager.Api;

namespace Signal.Api.ApiConfig
{
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
            return this.router.Route(req.HttpContext);
        }
    }
}
