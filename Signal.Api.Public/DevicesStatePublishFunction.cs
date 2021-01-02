using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Signal.Api.Public.Auth;
using Signal.Core;

namespace Signal.Api.Public
{
    public class SignalDeviceStatePublishDto
    {
        public string? DeviceIdentifier { get; set; }
        
        public string? ChannelName { get; set; }
        
        public string? ContactName { get; set; }
        
        public string? ValueSerialized { get; set; }
        
        public DateTime? TimeStamp { get; set; }
    }
    
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
                
                var requestContent = await new StreamReader(req.Body).ReadToEndAsync();
                var request = JsonSerializer.Deserialize<SignalDeviceStatePublishDto>(requestContent,
                    new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
                if (request == null)
                    throw new ExpectedHttpException(HttpStatusCode.BadRequest, "Failed to read request data.");
                
                // Publish state 
                await this.storage.QueueItemAsync(
                    QueueNames.DevicesState,
                    new DeviceStateQueueItem(
                        Guid.NewGuid(),
                        user.UserId,
                        request.TimeStamp ?? DateTime.UtcNow,
                        request.DeviceIdentifier,
                        request.ChannelName,
                        request.ContactName,
                        request.ValueSerialized),
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
            catch(Exception ex)
            {
                log.LogError(ex, "Failed to publish state.");
                throw;
            }
        }
    }
}