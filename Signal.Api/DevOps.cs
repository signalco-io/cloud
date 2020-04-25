using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Signal.Infrastructure.AzureDevOps;

namespace Signal.Api
{
    public static class DevOps
    {
        [FunctionName("devops-project-list")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            var data = await Class1.GetProjectsAsync();

            return new OkObjectResult(data );
        }
    }
}