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
using Signal.Core.Devices;
using Signal.Core.Exceptions;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Devices;

public class DevicesInfoUpdateFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IAzureStorage storage;
    private readonly IAzureStorageDao storageDao;

    public DevicesInfoUpdateFunction(
        IFunctionAuthenticator functionAuthenticator,
        IAzureStorage storage,
        IAzureStorageDao storageDao)
    {
        this.functionAuthenticator =
            functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        this.storageDao = storageDao ?? throw new ArgumentNullException(nameof(storageDao));
    }

    [FunctionName("Devices-InfoUpdate")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "devices/info/update")]
        HttpRequest req,
        CancellationToken cancellationToken) =>
        await req.UserRequest<DeviceInfoUpdateDto>(this.functionAuthenticator, async (user, payload) =>
        {
            if (string.IsNullOrWhiteSpace(payload.DeviceId))
                throw new ExpectedHttpException(
                    HttpStatusCode.BadRequest,
                    "DeviceId property is required.");
            if (string.IsNullOrWhiteSpace(payload.Alias))
                throw new ExpectedHttpException(
                    HttpStatusCode.BadRequest,
                    "Alias property is required.");

            // Check if user has assigned requested device
            if (!await this.storageDao.IsUserAssignedAsync(user.UserId, TableEntityType.Device, payload.DeviceId, cancellationToken))
                throw new ExpectedHttpException(HttpStatusCode.NotFound);

            // Commit endpoints
            await this.storage.UpdateItemAsync(
                ItemTableNames.Devices,
                new DeviceInfoUpdateTableEntity(
                    payload.DeviceId,
                    payload.Alias, 
                    payload.Manufacturer,
                    payload.Model),
                cancellationToken);
        }, cancellationToken);

    private class DeviceInfoUpdateDto
    {
        public string? DeviceId { get; set; }

        public string? Alias { get; set; }

        public string? Manufacturer { get; set; }

        public string? Model { get; set; }
    }
}