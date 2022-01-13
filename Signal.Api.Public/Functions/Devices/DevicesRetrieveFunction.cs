using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Signal.Api.Common;
using Signal.Api.Public.Auth;
using Signal.Api.Public.Exceptions;
using Signal.Core;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Devices;

public class DevicesRetrieveFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IEntityService entityService;
    private readonly IAzureStorageDao storage;

    public DevicesRetrieveFunction(
        IFunctionAuthenticator functionAuthenticator,
        IEntityService entityService,
        IAzureStorageDao storage)
    {
        this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    [FunctionName("Devices-Retrieve")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "devices")]
        HttpRequest req,
        CancellationToken cancellationToken) =>
        await req.UserRequest(cancellationToken, this.functionAuthenticator, async context =>
        {
            var devices = (await this.storage.DevicesAsync(context.User.UserId, cancellationToken)).ToList();
            var states = await this.storage.GetDeviceStatesAsync(devices.Select(d => d.RowKey).ToList(), cancellationToken);
            var entityUsers = await this.entityService.EntityUsersAsync(
                TableEntityType.Device, 
                devices.Select(d => d.RowKey), 
                cancellationToken);

            return devices.Select(d =>
            {
                var users = entityUsers[d.RowKey].Select(u => new UserDto
                {
                    Email = u.Email,
                    FullName = u.FullName,
                    Id = u.RowKey
                });

                return new DeviceDto(d.RowKey, d.DeviceIdentifier, d.Alias)
                {
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
        });

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
        
        public IEnumerable<DeviceContactStateDto>? States { get; set; }

        public IEnumerable<UserDto> SharedWith { get; set; }
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