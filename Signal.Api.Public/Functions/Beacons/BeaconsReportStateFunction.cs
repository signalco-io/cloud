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
using Signal.Api.Common.Auth;
using Signal.Api.Common.Exceptions;
using Signal.Core;
using Signal.Core.Beacon;
using Signal.Core.Exceptions;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Beacons;

public class BeaconsReportStateFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IEntityService entityService;
    private readonly IAzureStorageDao storageDao;
    private readonly IAzureStorage storage;

    public BeaconsReportStateFunction(
        IFunctionAuthenticator functionAuthenticator,
        IEntityService entityService,
        IAzureStorageDao storageDao,
        IAzureStorage storage)
    {
        this.functionAuthenticator =
            functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        this.storageDao = storageDao ?? throw new ArgumentNullException(nameof(storageDao));
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    [FunctionName("Beacons-ReportState")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "beacons/report-state")]
        HttpRequest req,
        CancellationToken cancellationToken) =>
        await req.UserRequest<BeaconReportStateRequestDto>(cancellationToken, this.functionAuthenticator, async context =>
        {
            var payload = context.Payload;
            var user = context.User;
            if (payload.Id == null)
                throw new ExpectedHttpException(HttpStatusCode.BadRequest, "BeaconId is required.");

            await context.ValidateUserAssignedAsync(this.entityService, TableEntityType.Station, payload.Id);
            
            await this.storage.UpdateItemAsync(
                ItemTableNames.Beacons,
                new BeaconStateItem(user.UserId, payload.Id)
                {
                    StateTimeStamp = DateTime.UtcNow,
                    Version = payload.Version,
                    AvailableWorkerServices = payload.AvailableWorkerServices != null
                        ? JsonSerializer.Serialize(payload.AvailableWorkerServices)
                        : null,
                    RunningWorkerServices = payload.RunningWorkerServices != null
                        ? JsonSerializer.Serialize(payload.RunningWorkerServices)
                        : null
                }, cancellationToken);
        });

    private class BeaconReportStateRequestDto
    {
        [Required] public string? Id { get; set; }

        [Required] public string? Version { get; set; }

        [Required] public List<string>? AvailableWorkerServices { get; set; }

        [Required] public List<string>? RunningWorkerServices { get; set; }
    }
}