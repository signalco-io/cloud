using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace Signal.Api.Internal.Functions;

public class StatusFunction
{
    [FunctionName("Status")]
    [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
    [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "API status is running.")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "status")] HttpRequest _) =>
        new OkResult();
}