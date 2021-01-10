using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Signal.Api.Public.Auth;
using Signal.Api.Public.Exceptions;
using Signal.Core;

namespace Signal.Api.Public.Functions
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
            CancellationToken cancellationToken) =>
            await req.UserRequest<SignalDeviceStatePublishDto>(this.functionAuthenticator, async (user, payload) =>
            {
                if (string.IsNullOrWhiteSpace(payload.ChannelName) ||
                    string.IsNullOrWhiteSpace(payload.ContactName) ||
                    string.IsNullOrWhiteSpace(payload.DeviceId))
                    throw new ExpectedHttpException(
                        HttpStatusCode.BadRequest,
                        "DeviceId, ChannelName and ContactName properties are required.");

                // Publish state 
                await this.storage.QueueItemAsync(
                    QueueNames.DevicesState,
                    new DeviceStateQueueItem(
                        user.UserId,
                        payload.DeviceId,
                        payload.ChannelName,
                        payload.ContactName,
                        payload.ValueSerialized,
                        payload.TimeStamp ?? DateTime.UtcNow),
                    cancellationToken);
            }, cancellationToken);

        private class SignalDeviceStatePublishDto
        {
            public string? DeviceId { get; set; }

            public string? ChannelName { get; set; }

            public string? ContactName { get; set; }

            public string? ValueSerialized { get; set; }

            public DateTime? TimeStamp { get; set; }
        }
    }
}