using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Signal.Api.Public.Functions.Devices
{
    public class DevicesRetrieveFunction
    {
        private readonly IFunctionAuthenticator functionAuthenticator;
        private readonly IAzureStorageDao storage;

        public DevicesRetrieveFunction(
            IFunctionAuthenticator functionAuthenticator,
            IAzureStorageDao storage)
        {
            this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        [FunctionName("Devices-Retrieve")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "devices")]
            HttpRequest req,
            CancellationToken cancellationToken) =>
            await req.UserRequest(this.functionAuthenticator, async user =>
            {
                var devices = await this.storage.DevicesAsync(user.UserId, cancellationToken);
                return devices.Select(d => new DeviceDto(d.RowKey, d.DeviceIdentifier, d.Alias)
                {
                    Endpoints = d.Endpoints != null
                        ? JsonSerializer.Deserialize<IEnumerable<DeviceEndpointDto>>(d.Endpoints,
                            new JsonSerializerOptions {PropertyNameCaseInsensitive = true})
                        : null,
                    Manufacturer = d.Manufacturer,
                    Model = d.Model
                });
            }, cancellationToken);

        private class DeviceDto
        {
            public DeviceDto(string id, string deviceIdentifier, string alias)
            {
                this.Id = id;
                this.DeviceIdentifier = deviceIdentifier;
                this.Alias = alias;
            }

            public string Id { get; set; }

            public string DeviceIdentifier { get; set; }

            public string Alias { get; set; }

            public IEnumerable<DeviceEndpointDto>? Endpoints { get; set; }

            public string? Manufacturer { get; set; }

            public string? Model { get; set; }
        }
    }
}