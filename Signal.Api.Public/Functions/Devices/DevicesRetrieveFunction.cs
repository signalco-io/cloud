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
using Signal.Api.Common.Auth;
using Signal.Api.Common.Exceptions;
using Signal.Core.Entities;
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
            var devices = (await this.storage.UserEntitiesAsync(context.User.UserId, cancellationToken)).ToList();
            var states = await this.storage.ContactsAsync(devices.Select(d => d.Id).ToList(), cancellationToken);
            var entityUsers = await this.entityService.EntityUsersAsync(
                devices.Select(d => d.Id),
                cancellationToken);

            return devices.Select(d =>
            {
                var users = entityUsers[d.Id].Select(u => new UserDto
                {
                    Email = u.Email,
                    FullName = u.FullName,
                    Id = u.UserId
                });

                return new EntityDto(d.Id, d.Alias)
                {
                    States = states.Where(s => s.EntityId == d.Id).Select(s => new DeviceContactStateDto
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

    [Serializable]
    private class EntityDto
    {
        public EntityDto(string id, string alias)
        {
            this.Id = id;
            this.Alias = alias;
        }

        public string Id { get; }

        public string Alias { get; }

        public IEnumerable<DeviceContactStateDto>? States { get; set; }

        public IEnumerable<UserDto>? SharedWith { get; set; }
    }

    [Serializable]
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