using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Signal.Api.Common;
using Signal.Api.Public.Auth;
using Signal.Api.Public.Exceptions;
using Signal.Core;
using Signal.Core.Devices;
using Signal.Core.Exceptions;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Devices
{
    public class DevicesStatePublishFunction
    {
        private readonly IFunctionAuthenticator functionAuthenticator;
        private readonly IAzureStorage storage;
        private readonly IAzureStorageDao dao;

        public DevicesStatePublishFunction(
            IFunctionAuthenticator functionAuthenticator,
            IAzureStorage storage,
            IAzureStorageDao dao)
        {
            this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this.dao = dao ?? throw new ArgumentNullException(nameof(dao));
        }

        [FunctionName("Devices-PublishState")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "devices/state")]
            HttpRequest req,
            [SignalR(HubName = "devices")] 
            IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger logger,
            CancellationToken cancellationToken) =>
            await req.UserRequest<SignalDeviceStatePublishDto>(this.functionAuthenticator, async (user, payload) =>
            {
                if (string.IsNullOrWhiteSpace(payload.ChannelName) ||
                    string.IsNullOrWhiteSpace(payload.ContactName) ||
                    string.IsNullOrWhiteSpace(payload.DeviceId))
                    throw new ExpectedHttpException(
                        HttpStatusCode.BadRequest,
                        "DeviceId, ChannelName and ContactName properties are required.");
                
                // Validate user assigned
                var isOwned = await this.dao.IsUserAssignedAsync(
                    user.UserId,
                    TableEntityType.Device,
                    payload.DeviceId,
                    cancellationToken);
                if (!isOwned)
                    throw new ExpectedHttpException(HttpStatusCode.NotFound);

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

                // Wait for current state update before triggering notification
                await updateCurrentStateTask;

                Task? persistHistoryTask = null;
                Task? notifyStateChangeTask = null;
                try
                {
                    // Persist to history 
                    // TODO: persist only if given contact is marked for history tracking
                    persistHistoryTask = this.storage.CreateOrUpdateItemAsync(
                        ItemTableNames.DevicesStatesHistory,
                        new DeviceStateHistoryTableEntity(
                            payload.DeviceId,
                            payload.ChannelName,
                            payload.ContactName,
                            payload.ValueSerialized,
                            payload.TimeStamp ?? DateTime.UtcNow),
                        cancellationToken);

                    // Notify listeners
                    notifyStateChangeTask = signalRMessages.AddAsync(new SignalRMessage
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
                }
                catch (Exception ex)
                {
                    if (notifyStateChangeTask?.IsFaulted ?? false)
                        logger.LogError(ex, "Failed to notify state change.");
                    else if (persistHistoryTask?.IsFaulted ?? false)
                        logger.LogError(ex, "Failed to persist state to history.");
                    else logger.LogError(ex, "Failed to notify or persist state to history.");
                }
            }, cancellationToken);
    }
}