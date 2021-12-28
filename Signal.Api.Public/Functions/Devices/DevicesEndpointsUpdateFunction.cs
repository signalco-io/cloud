using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Signal.Api.Public.Auth;
using Signal.Api.Public.Exceptions;
using Signal.Api.Public.Functions.Devices.Dtos;
using Signal.Core;
using Signal.Core.Devices;
using Signal.Core.Exceptions;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Devices;

public class DevicesEndpointsUpdateFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IAzureStorage storage;
    private readonly IAzureStorageDao storageDao;

    public DevicesEndpointsUpdateFunction(
        IFunctionAuthenticator functionAuthenticator,
        IAzureStorage storage,
        IAzureStorageDao storageDao)
    {
        this.functionAuthenticator =
            functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        this.storageDao = storageDao ?? throw new ArgumentNullException(nameof(storageDao));
    }

    [FunctionName("Devices-EndpointsUpdate")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "devices/endpoints/update")]
        HttpRequest req,
        CancellationToken cancellationToken) =>
        await req.UserRequest<DeviceEndpointsUpdateDto>(this.functionAuthenticator, async (user, payload) =>
        {
            if (string.IsNullOrWhiteSpace(payload.DeviceId))
                throw new ExpectedHttpException(
                    HttpStatusCode.BadRequest,
                    "DeviceIdentifier property is required.");

            // Check if user has assigned requested device
            if (!await this.storageDao.IsUserAssignedAsync(user.UserId, TableEntityType.Device, payload.DeviceId,
                    cancellationToken))
                throw new ExpectedHttpException(HttpStatusCode.NotFound);
                
            // Commit endpoints
            await this.storage.CreateOrUpdateItemAsync(
                ItemTableNames.Devices,
                new DeviceTableEndpointsEntity(
                    payload.DeviceId,
                    JsonSerializer.Serialize(payload.Endpoints)),
                cancellationToken);
        }, cancellationToken);

    [Serializable]
    private class DeviceEndpointsUpdateDto
    {
        public string? DeviceId { get; set; }

        [Required]
        public IEnumerable<DeviceEndpointDto>? Endpoints { get; set; }
    }
}