using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
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

public class StationsLoggingPersistFunction
    {
        private readonly IFunctionAuthenticator functionAuthenticator;
        private readonly IAzureStorage storage;
        
        public StationsLoggingPersistFunction(
            IFunctionAuthenticator functionAuthenticator,
            IAzureStorage storage)
        {
            this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        [FunctionName("Stations-Logging-Persist")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "stations/logging/persist")]
            HttpRequest req,
            CancellationToken cancellationToken) =>
            await req.UserRequest<StationsLoggingPersistRequestDto>(this.functionAuthenticator, async (user, payload) =>
                {
                    // TODO: Check if user owns station
                    
                    var entriesByDate = (payload.Entries ?? Enumerable.Empty<StationsLoggingPersistRequestDto.Entry>())
                        .Where(e => e.TimeStamp.HasValue)
                        .GroupBy(e => e.TimeStamp!.Value.Date)
                        .ToList();
                    foreach (var entriesDay in entriesByDate)
                    {
                        var sb = new StringBuilder();
                        foreach (var entry in entriesDay)
                            sb.AppendLine($"[{entry.TimeStamp:O}] ({entry.Level.ToString()}) {entry.Message}");

                        var fileName = $"{payload.StationId}-{entriesDay.Key:yyyyMMdd}.log";
                        var logs = sb.ToString();
                        await this.storage.AppendToFileAsync("Logs/Stations/", fileName, logs, cancellationToken);
                    }
                },
                cancellationToken);

        private class StationsLoggingPersistRequestDto
        {
            public string? StationId { get; set; }
            
            public List<Entry>? Entries { get; set; }
            
            public class Entry
            {
                [JsonPropertyName("T")]
                public DateTime? TimeStamp { get; set; }
                
                [JsonPropertyName("L")]
                public LogLevel? Level { get; set; }
                
                [JsonPropertyName("M")]
                public string? Message { get; set; }
            }

            public enum LogLevel
            {
                Trace,
                Debug,
                Information,
                Warning,
                Error,
                Fatal
            }
        }
    }

public class BeaconsReportStateFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IAzureStorageDao storageDao;
    private readonly IAzureStorage storage;


    public class BeaconsReportStateFunction
    {
        private readonly IFunctionAuthenticator functionAuthenticator;
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