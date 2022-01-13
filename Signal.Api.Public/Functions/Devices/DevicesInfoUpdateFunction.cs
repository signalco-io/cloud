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
    private readonly IEntityService entityService;

    public DevicesInfoUpdateFunction(
        IFunctionAuthenticator functionAuthenticator,
        IAzureStorage storage,
        IEntityService entityService)
    {
        this.functionAuthenticator =
            functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
    }

    [FunctionName("Devices-InfoUpdate")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "devices/info/update")]
        HttpRequest req,
        CancellationToken cancellationToken) =>
        await req.UserRequest<DeviceInfoUpdateDto>(cancellationToken, this.functionAuthenticator, async context =>
        {
            var payload = context.Payload;
            if (string.IsNullOrWhiteSpace(payload.DeviceId))
                throw new ExpectedHttpException(
                    HttpStatusCode.BadRequest,
                    "DeviceId property is required.");
            if (string.IsNullOrWhiteSpace(payload.Alias))
                throw new ExpectedHttpException(
                    HttpStatusCode.BadRequest,
                    "Alias property is required.");

            await context.ValidateUserAssignedAsync(this.entityService, TableEntityType.Device, payload.DeviceId);

            // Commit endpoints
            await this.storage.UpdateItemAsync(
                ItemTableNames.Devices,
                new DeviceInfoUpdateTableEntity(
                    payload.DeviceId,
                    payload.Alias),
                cancellationToken);
        });

    private class DeviceInfoUpdateDto
    {
        public string? DeviceId { get; set; }

        public string? Alias { get; set; }
    }
}