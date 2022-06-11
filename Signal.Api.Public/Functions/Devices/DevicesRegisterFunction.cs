using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Signal.Api.Common.Auth;
using Signal.Api.Common.Exceptions;
using Signal.Core.Entities;
using Signal.Core.Exceptions;
using Signal.Core.Storage;
using Signal.Infrastructure.AzureStorage.Tables;

namespace Signal.Api.Public.Functions.Devices;

public class DevicesRegisterFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IAzureStorageDao storageDao;
    private readonly IEntityService entityService;

    public DevicesRegisterFunction(
        IFunctionAuthenticator functionAuthenticator,
        IAzureStorageDao storageDao,
        IEntityService entityService)
    {
        this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.storageDao = storageDao ?? throw new ArgumentNullException(nameof(storageDao));
        this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
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

            var deviceId = await this.entityService.UpsertAsync(
                user.UserId,
                null,
                TableEntityType.Device,
                ItemTableNames.Devices,
                id => new DeviceInfoTableEntity(
                    id,
                    payload.DeviceIdentifier,
                    payload.Alias ?? "New entity"),
                cancellationToken);

            return new DeviceRegisterResponseDto(deviceId);
        });

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