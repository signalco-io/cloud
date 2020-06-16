using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Signal.Api
{
    /// <summary>
    /// A ping.
    /// </summary>
    public static class Ping
    {
        /// <summary>
        /// Runs the given request.
        /// </summary>
        /// <param name="req">The request.</param>
        /// <returns>
        /// An asynchronous result that yields an IActionResult.
        /// </returns>
        [FunctionName("Ping")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest _)
        {
            var data = new
            {
                Version = typeof(Ping).Assembly.GetName().Version?.ToString()
            };

            return new OkObjectResult(data);
        }
    }
}
