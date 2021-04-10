using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Signal.Api.Public.Auth;
using Signal.Api.Public.Exceptions;
using Signal.Core;

namespace Signal.Api.Public.Functions.Devices
{
    public class DevicesStateHistoryRetrieveFunction
    {
        private readonly IFunctionAuthenticator functionAuthenticator;
        private readonly IAzureStorageDao storage;

        public DevicesStateHistoryRetrieveFunction(
            IFunctionAuthenticator functionAuthenticator,
            IAzureStorageDao storage)
        {
            this.functionAuthenticator =
                functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        [FunctionName("Devices-RetrieveStateHistory")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "devices/state-history")]
            HttpRequest req,
            CancellationToken cancellationToken) =>
            await req.UserRequest<DeviceStateHistoryResponseDto>(this.functionAuthenticator, async user =>
            {
                var deviceId = req.Query["deviceId"];
                var channelName = req.Query["channelName"];
                var contactName = req.Query["contactName"];
                var duration = req.Query["duration"];

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
            }, cancellationToken);

        private class DeviceStateHistoryRequestDto
        {
            public string? DeviceId { get; set; }

            public string? ChannelName { get; set; }

            public string? ContactName { get; set; }

            public TimeSpan? Duration { get; set; }
        }

        private class DeviceStateHistoryResponseDto
        {
            public List<TimeStampValuePair> Values { get; set; } = new List<TimeStampValuePair>();

            public class TimeStampValuePair
            {
                public DateTime? TimeStamp { get; set; }

                public string? ValueSerialized { get; set; }
            }
        }
    }
}