using System;
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
using Signal.Core.Devices;
using Signal.Core.Exceptions;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Devices;

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
        await req.UserRequest<DeviceRegisterDto, DeviceRegisterResponseDto>(cancellationToken, this.functionAuthenticator, async context =>
        {
            var payload = context.Payload;
            var user = context.User;
            if (string.IsNullOrWhiteSpace(payload.DeviceIdentifier))
                throw new ExpectedHttpException(
                    HttpStatusCode.BadRequest,
                    "DeviceIdentifier property is required.");

            // Check if device already exists
            var userDevices = await this.storageDao.DevicesAsync(user.UserId, cancellationToken);
            if (userDevices.Any(ud => ud.DeviceIdentifier == payload.DeviceIdentifier))
                throw new ExpectedHttpException(HttpStatusCode.BadRequest, "Device already exists.");

            // TODO: Move to EntityService.GenerateIdAsync(EntityType, CancellationToken)
            // Generate device id
            // Check if device with new id exists (avoid collisions)
            var deviceId = Guid.NewGuid().ToString();
            while ((await this.storageDao.EntitiesByRowKeysAsync(ItemTableNames.Devices, new[] { deviceId }, cancellationToken)).Any())
                deviceId = Guid.NewGuid().ToString();

            // Create new device
            await this.storage.CreateOrUpdateItemAsync(
                ItemTableNames.Devices,
                new DeviceInfoTableEntity(
                    deviceId,
                    payload.DeviceIdentifier,
                    payload.Alias ?? "New device",
                    payload.Manufacturer,
                    payload.Model),
                cancellationToken);

            // Assign device to user
            await this.storage.CreateOrUpdateItemAsync(
                ItemTableNames.UserAssignedEntity(TableEntityType.Device),
                new UserAssignedEntityTableEntry(
                    user.UserId,
                    deviceId),
                cancellationToken);

            return new DeviceRegisterResponseDto(deviceId);
        });

    private class DeviceRegisterDto
    {
        public string? DeviceIdentifier { get; set; }

        public string? Alias { get; set; }
            
        public string? Manufacturer { get; set; }

        public string? Model { get; set; }
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