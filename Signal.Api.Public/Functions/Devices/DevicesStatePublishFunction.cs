using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Signal.Api.Common;
using Signal.Api.Public.Auth;
using Signal.Api.Public.Exceptions;
using Signal.Core;

namespace Signal.Api.Public.Functions.Devices
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
            [SignalR(HubName = "devices")] 
            IAsyncCollector<SignalRMessage> signalRMessages,
            CancellationToken cancellationToken) =>
            await req.UserRequest<SignalDeviceStatePublishDto>(this.functionAuthenticator, async (user, payload) =>
            {
                if (string.IsNullOrWhiteSpace(payload.ChannelName) ||
                    string.IsNullOrWhiteSpace(payload.ContactName) ||
                    string.IsNullOrWhiteSpace(payload.DeviceId))
                    throw new ExpectedHttpException(
                        HttpStatusCode.BadRequest,
                        "DeviceId, ChannelName and ContactName properties are required.");
                
                // TODO: Validate user assigned
                // TODO: Retrieve device configuration
                // TODO: Validate device has contact for state
                // TODO: Assign to device's ignore states if not assigned in config (update device for state visibility)

                // Persist as current state
                var updateCurrentStateTask = this.storage.CreateOrUpdateItemAsync(
                    ItemTableNames.DeviceStates,
                    new DeviceStateTableEntity(
                        payload.DeviceId,
                        payload.ChannelName,
                        payload.ContactName,
                        payload.ValueSerialized,
                        payload.TimeStamp ?? DateTime.UtcNow),
                    cancellationToken);

                // Persist to history 
                // TODO: persist only if given contact is marked for history tracking
                var persistHistoryTask = this.storage.CreateOrUpdateItemAsync(
                    ItemTableNames.DevicesStatesHistory,
                    new DeviceStateHistoryTableEntity(
                        payload.DeviceId,
                        payload.ChannelName,
                        payload.ContactName,
                        payload.ValueSerialized,
                        payload.TimeStamp ?? DateTime.UtcNow),
                    cancellationToken);

                // Wait for current state update before triggering notification
                await updateCurrentStateTask;

                // Notify listeners
                var notifyStateChangeTask = signalRMessages.AddAsync(new SignalRMessage
                {
                    UserId = user.UserId,
                    Arguments = new object[]
                    {
                        new SignalDeviceStatePublishDto
                        {
                            DeviceId = payload.DeviceId,
                            ChannelName = payload.ChannelName,
                            ContactName = payload.ContactName,
                            TimeStamp = payload.TimeStamp,
                            ValueSerialized = payload.ValueSerialized
                        }
                    },
                    Target = "devicestate"
                }, cancellationToken);

                // Wait for all to finish
                await Task.WhenAll(
                    notifyStateChangeTask,
                    persistHistoryTask);
            }, cancellationToken);
    }
}