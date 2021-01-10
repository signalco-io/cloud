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
using Signal.Api.Public.Exceptions;
using Signal.Core;

namespace Signal.Api.Public.Functions
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "devices/register")]
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
                // Check if device with new id exists (avoid collisions)
                var deviceId = Guid.NewGuid().ToString();
                while (await this.storageDao.DeviceExistsAsync(deviceId, cancellationToken))
                    deviceId = Guid.NewGuid().ToString();

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

        private class DeviceRegisterDto
        {
            public string? DeviceIdentifier { get; set; }

            public string? Alias { get; set; }
        }

        private class DeviceRegisterResponseDto
        {
            public DeviceRegisterResponseDto(string deviceId)
            {
                this.DeviceId = deviceId;
            }

            public string DeviceId { get; }
        }
    }
}