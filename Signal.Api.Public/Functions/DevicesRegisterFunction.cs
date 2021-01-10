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
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "devices/register")]
            HttpRequest req,
            CancellationToken cancellationToken) =>
            await req.UserRequest<DeviceRegisterDto, DeviceRegisterResponseDto>(this.functionAuthenticator, async (user, payload) =>
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
                    throw new ExpectedHttpException(HttpStatusCode.BadRequest, "Device already exists.");

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
                await this.storage.CreateOrUpdateItemAsync(
                    ItemTableNames.Users,
                    new UserAssignedEntityTableEntry(
                        user.UserId,
                        EntityType.Device,
                        deviceId),
                    cancellationToken);

                return new DeviceRegisterResponseDto(deviceId);
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