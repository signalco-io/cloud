using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Signal.Core;

namespace Signal.Api.Public
{
    public class DevicesStatePublishFunction
    {
        private readonly IFunctionAuthenticator functionAuthenticator;
        private readonly IAzureStorage storage;

        public DevicesStatePublishFunction(
            IFunctionAuthenticator functionAuthenticator,
            IAzureStorage storage)
        {
            this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        [FunctionName("Devices-PublishState")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "devices/state")]
            HttpRequest req,
            ILogger log,
            CancellationToken cancellationToken)
        {
            try
            {
                var user = await this.functionAuthenticator.AuthenticateAsync(req, cancellationToken);
                
                // Publish state 
                await this.storage.QueueMessageAsync(
                    QueueNames.DevicesState,
                    new DeviceStateQueueItem(
                        Guid.NewGuid(),
                        user.UserId,
                        DateTime.UtcNow,
                        "test",
                        "main",
                        "state",
                        "true"),
                    cancellationToken);

                return new OkResult();
            }
            catch (ExpectedHttpException ex)
            {
                return new ObjectResult(new ApiErrorDto(ex.Code.ToString(), ex.Message))
                {
                    StatusCode = (int)ex.Code
                };
            }
        }
    }
}