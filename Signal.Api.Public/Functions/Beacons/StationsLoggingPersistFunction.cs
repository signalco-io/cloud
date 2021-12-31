using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Signal.Api.Common;
using Signal.Api.Public.Auth;
using Signal.Api.Public.Exceptions;
using Signal.Core;
using Signal.Core.Exceptions;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Beacons;

public class StationsLoggingPersistFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IEntityService entityService;
    private readonly IAzureStorage storage;

    public StationsLoggingPersistFunction(
        IFunctionAuthenticator functionAuthenticator,
        IEntityService entityService,
        IAzureStorage storage)
    {
        this.functionAuthenticator =
            functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    [FunctionName("Stations-Logging-Persist")]
    [OpenApiSecurityAuth0Token]
    [OpenApiOperation(nameof(StationsLoggingPersistFunction), "Stations", Description = "Appends logging entries.")]
    [OpenApiRequestBody("application/json", typeof(StationsLoggingPersistRequestDto), Description = "The logging entries to persist per station.")]
    [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
    [OpenApiResponseBadRequestValidation]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "stations/logging/persist")]
        HttpRequest req,
        CancellationToken cancellationToken) =>
        await req.UserRequest<StationsLoggingPersistRequestDto>(
            cancellationToken, this.functionAuthenticator, async context =>
            {
                var payload = context.Payload;
                if (string.IsNullOrWhiteSpace(payload.StationId))
                    throw new ExpectedHttpException(HttpStatusCode.BadRequest, "StationId is required.");

                await context.ValidateUserAssignedAsync(
                    this.entityService,
                    TableEntityType.Station,
                    payload.StationId);

                var entriesByDate = (payload.Entries ?? Enumerable.Empty<StationsLoggingPersistRequestDto.Entry>())
                    .Where(e => e.TimeStamp.HasValue)
                    .GroupBy(e => e.TimeStamp!.Value.Date)
                    .ToList();
                foreach (var entriesDay in entriesByDate)
                {
                    var sb = new StringBuilder();
                    foreach (var entry in entriesDay)
                        sb.AppendLine((string?) $"[{entry.TimeStamp:O}] ({entry.Level}) {entry.Message}");

                    var fileName = $"{entriesDay.Key:yyyyMMdd}.txt";

                    await using var ms = new MemoryStream();
                    await using var sw = new StreamWriter(ms, Encoding.UTF8);
                    await sw.WriteAsync(sb, cancellationToken);
                    await sw.FlushAsync();
                    ms.Position = 0;

                    await this.storage.AppendToFileAsync(payload.StationId.SanitizeFileName(), fileName, ms, cancellationToken);
                }
            });

    private class StationsLoggingPersistRequestDto
    {
        public string? StationId { get; set; }

        public List<Entry>? Entries { get; set; }

        public class Entry
        {
            [JsonPropertyName("T")] public DateTimeOffset? TimeStamp { get; set; }

            [JsonPropertyName("L")] public LogLevel? Level { get; set; }

            [JsonPropertyName("M")] public string? Message { get; set; }
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