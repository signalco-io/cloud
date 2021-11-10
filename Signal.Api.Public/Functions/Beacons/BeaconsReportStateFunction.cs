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
using Signal.Core;
using Signal.Core.Beacon;
using Signal.Core.Exceptions;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Beacons;

public class BeaconsReportStateFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IAzureStorageDao storageDao;
    private readonly IAzureStorage storage;

    public BeaconsReportStateFunction(
        IFunctionAuthenticator functionAuthenticator,
        IAzureStorageDao storageDao,
        IAzureStorage storage)
    {
        this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.storageDao = storageDao ?? throw new ArgumentNullException(nameof(storageDao));
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    [FunctionName("Beacons-ReportState")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "beacons/report-state")]
        HttpRequest req,
        CancellationToken cancellationToken) =>
        await req.UserRequest<BeaconReportStateRequestDto>(this.functionAuthenticator, async (user, payload) =>
        {
            if (payload.Id == null)
                throw new ExpectedHttpException(HttpStatusCode.BadRequest, "BeaconId is required.");

            // Check if beacon is assigned to user
            if (!await this.storageDao.IsUserAssignedAsync(
                    user.UserId, 
                    TableEntityType.Station, 
                    payload.Id,
                    cancellationToken))
                throw new ExpectedHttpException(HttpStatusCode.NotFound);

            await this.storage.UpdateItemAsync(
                ItemTableNames.Beacons,
                new BeaconStateItem(user.UserId, payload.Id)
                {
                    StateTimeStamp = DateTime.UtcNow,
                    Version = payload.Version,
                    AvailableWorkerServices = payload.AvailableWorkerServices != null ? JsonSerializer.Serialize(payload.AvailableWorkerServices) : null,
                    RunningWorkerServices = payload.RunningWorkerServices != null ? JsonSerializer.Serialize(payload.RunningWorkerServices) : null
                }, cancellationToken);
        }, cancellationToken);

    private class BeaconReportStateRequestDto
    {
        [Required]
        public string? Id { get; set; }

        [Required]
        public string? Version { get; set; }

        [Required]
        public List<string>? AvailableWorkerServices { get; set; }

        [Required]
        public List<string>? RunningWorkerServices { get; set; }
    }
}