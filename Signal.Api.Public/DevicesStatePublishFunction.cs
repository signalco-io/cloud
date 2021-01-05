using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Signal.Api.Public.Auth;
using Signal.Core;

namespace Signal.Api.Public
{
    public class DevicesRegisterFunction
    {
        private readonly IFunctionAuthenticator functionAuthenticator;
        private readonly IAzureStorage storage;
        private readonly IAzureStorageDao storageDao;

        public DevicesRegisterFunction(
            IFunctionAuthenticator functionAuthenticator,
            IAzureStorage storage,
            IAzureStorageDao storageDao)
        {
            this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this.storageDao = storageDao ?? throw new ArgumentNullException(nameof(storageDao));
        }

        [FunctionName("Devices-Register")]
        public async Task Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "devices/state")]
            HttpRequest req,
            CancellationToken cancellationToken) =>
            await req.UserRequest<DeviceRegisterDto>(this.functionAuthenticator, async (user, payload) =>
            {
                if (string.IsNullOrWhiteSpace(payload.DeviceIdentifier))
                    throw new ExpectedHttpException(
                        HttpStatusCode.BadRequest,
                        "DeviceIdentifier property is required.");

                // Check if device already exists
                var existingDeviceId = await this.storageDao.DeviceExistsAsync(
                    user.UserId,
                    payload.DeviceIdentifier,
                    cancellationToken);
                if (!string.IsNullOrWhiteSpace(existingDeviceId))
                    return new BadRequestObjectResult(new DeviceRegisterResponseDto(existingDeviceId));
                
                // Generate device id
                var deviceId = Guid.NewGuid().ToString();
                
                // TODO: Check if device with new id exists (avoid collisions)

                // Create new device
                await this.storage.CreateOrUpdateItemAsync(ItemTableNames.Devices, new DeviceTableEntity(
                        deviceId,
                        payload.DeviceIdentifier,
                        payload.Alias ?? "New device"),
                    cancellationToken);

                // Assign device to user
                var currentUserAssignments = await this.storageDao.UserAsync(user.UserId, UserData.AssignedEntities, cancellationToken);
                await this.storage.CreateOrUpdateItemAsync(
                    ItemTableNames.Users,
                    new UserAssignedEntitiesTableEntry(
                        user.UserId,
                        UserData.AssignedEntities,
                        (currentUserAssignments?.Devices ?? new List<string>()).Concat(new[] {deviceId})),
                    cancellationToken);

                return new OkObjectResult(new DeviceRegisterResponseDto(deviceId));
            }, cancellationToken);
    }

    public class DeviceRegisterDto
    {
        public string? DeviceIdentifier { get; set; }

        public string? Alias { get; set; }
    }

    public class DeviceRegisterResponseDto
    {
        public DeviceRegisterResponseDto(string deviceId)
        {
            this.DeviceId = deviceId;
        }

        public string DeviceId { get; }
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
    }
}