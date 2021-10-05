using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
using Signal.Core.Exceptions;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Devices
{
    public class DevicesStateHistoryRetrieveFunction
    {
        private readonly IFunctionAuthenticator functionAuthenticator;
        private readonly IAzureStorageDao storageDao;
        private readonly IAzureStorageDao storage;

        public DevicesStateHistoryRetrieveFunction(
            IFunctionAuthenticator functionAuthenticator,
            IAzureStorageDao storageDao,
            IAzureStorageDao storage)
        {
            this.functionAuthenticator =
                functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
            this.storageDao = storageDao ?? throw new ArgumentNullException(nameof(storageDao));
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        [FunctionName("Devices-RetrieveStateHistory")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "devices/state-history")]
            HttpRequest req,
            CancellationToken cancellationToken) =>
            await req.UserRequest(this.functionAuthenticator, async user =>
            {
                var deviceId = req.Query["deviceId"];
                var channelName = req.Query["channelName"];
                var contactName = req.Query["contactName"];
                var duration = req.Query["duration"];

                // Check if user has assigned requested device
                if (!await this.storageDao.IsUserAssignedAsync(user.UserId, TableEntityType.Device, deviceId, cancellationToken))
                    throw new ExpectedHttpException(HttpStatusCode.NotFound);

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