using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace Signalco.Channel.Slack.Functions
{
    public class StatusFunction
    {
        [FunctionName("Status")]
        [OpenApiOperation(operationId: nameof(StatusFunction), tags: new[] { "Health" })]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "API is running.")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "status")] HttpRequest req) =>
            new OkResult();
    }
}
