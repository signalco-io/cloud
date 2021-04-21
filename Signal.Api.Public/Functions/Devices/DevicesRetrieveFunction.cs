using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
using Signal.Core.Storage;
using Signal.Core.Users;

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
                var devices = (await this.storage.DevicesAsync(user.UserId, cancellationToken)).ToList();
                var states = await this.storage.GetDeviceStatesAsync(devices.Select(d => d.RowKey).ToList(), cancellationToken);
                var assignedDevicesUsers = await this.storage.AssignedUsersAsync(
                    TableEntityType.Device, 
                    devices.Select(d => d.RowKey),
                    cancellationToken);
                var assignedUserIds = assignedDevicesUsers.Values.SelectMany(i => i).Distinct().ToList();
                var assignedUsers = new Dictionary<string, IUserTableEntity>();
                foreach (var userId in assignedUserIds)
                {
                    var assignedUser = await this.storage.UserAsync(userId, cancellationToken);
                    if (assignedUser != null)
                        assignedUsers.Add(assignedUser.RowKey, assignedUser);
                }

                return devices.Select(d =>
                {
                    var users = new List<UserDto>();
                    if (assignedDevicesUsers.TryGetValue(d.RowKey, out var assignedDeviceUserIds))
                    {
                        foreach (var assignedDeviceUserId in assignedDeviceUserIds)
                        {
                            if (assignedUsers.TryGetValue(assignedDeviceUserId, out var assignedDeviceUser))
                            {
                                users.Add(new UserDto
                                {
                                    Id = assignedDeviceUser.RowKey,
                                    FullName = assignedDeviceUser.FullName,
                                    Email = assignedDeviceUser.Email
                                });
                            }
                        }
                    }

                    return new DeviceDto(d.RowKey, d.DeviceIdentifier, d.Alias)
                    {
                        Endpoints = d.Endpoints != null
                            ? JsonSerializer.Deserialize<IEnumerable<DeviceEndpointDto>>(d.Endpoints,
                                new JsonSerializerOptions {PropertyNameCaseInsensitive = true})
                            : null,
                        Manufacturer = d.Manufacturer,
                        Model = d.Model,
                        States = states.Where(s => s.PartitionKey == d.RowKey).Select(s => new DeviceContactStateDto
                        (
                            s.ContactName,
                            s.ChannelName,
                            s.ValueSerialized,
                            s.TimeStamp
                        )),
                        SharedWith = users
                    };
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

            public string Id { get; }

            public string DeviceIdentifier { get; }

            public string Alias { get; }

            public IEnumerable<DeviceEndpointDto>? Endpoints { get; set; }

            public string? Manufacturer { get; set; }

            public string? Model { get; set; }

            public IEnumerable<DeviceContactStateDto>? States { get; set; }

            public IEnumerable<UserDto> SharedWith { get; set; }
        }

        private class UserDto
        {
            public string Id { get; set; }

            public string Email { get; set; }

            public string? FullName { get; set; }
        }

        private class DeviceContactStateDto
        {
            public DeviceContactStateDto(string name, string channel, string? valueSerialized, DateTime timeStamp)
            {
                this.Name = name;
                this.Channel = channel;
                this.ValueSerialized = valueSerialized;
                this.TimeStamp = timeStamp;
            }

            public string Name { get; }

            public string Channel { get; }

            public string? ValueSerialized { get; }

            public DateTime TimeStamp { get; }
        }
    }
}