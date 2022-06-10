using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Signal.Api.Common.Auth;
using Signal.Api.Common.Exceptions;
using Signal.Core;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Devices;

public class DevicesStateHistoryRetrieveFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IEntityService entityService;
    private readonly IAzureStorageDao storage;

    public DevicesStateHistoryRetrieveFunction(
        IFunctionAuthenticator functionAuthenticator,
        IEntityService entityService,
        IAzureStorageDao storage)
    {
        this.functionAuthenticator =
            functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    [FunctionName("Devices-RetrieveStateHistory")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "devices/state-history")]
        HttpRequest req,
        CancellationToken cancellationToken) =>
        await req.UserRequest(cancellationToken, this.functionAuthenticator, async context =>
        {
            var deviceId = req.Query["deviceId"];
            var channelName = req.Query["channelName"];
            var contactName = req.Query["contactName"];
            var duration = req.Query["duration"];

            await context.ValidateUserAssignedAsync(this.entityService, TableEntityType.Device, deviceId);

            var data = await this.storage.GetDeviceStateHistoryAsync(
                deviceId,
                channelName,
                contactName,
                TimeSpan.TryParse(duration, out var durationValue) ? durationValue : TimeSpan.FromDays(1),
                cancellationToken);

            return new DeviceStateHistoryResponseDto
            {
                Values = data.Select(d => new DeviceStateHistoryResponseDto.TimeStampValuePair
                {
                    TimeStamp = d.Timestamp?.UtcDateTime ?? DateTime.MinValue,
                    ValueSerialized = d.ValueSerialized
                }).ToList()
            };
        });

    private class DeviceStateHistoryRequestDto
    {
        public string? DeviceId { get; set; }

        public string? ChannelName { get; set; }

        public string? ContactName { get; set; }

        public TimeSpan? Duration { get; set; }
    }

    private class DeviceStateHistoryResponseDto
    {
        public List<TimeStampValuePair> Values { get; set; } = new();

        public class TimeStampValuePair
        {
            public DateTime? TimeStamp { get; set; }

            public string? ValueSerialized { get; set; }
        }
    }
}