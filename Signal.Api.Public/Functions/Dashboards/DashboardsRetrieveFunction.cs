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

namespace Signal.Api.Public.Functions.Dashboards;

public class DashboardsRetrieveFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IEntityService entityService;
    private readonly IAzureStorageDao storage;

    public DashboardsRetrieveFunction(
        IFunctionAuthenticator functionAuthenticator,
        IEntityService entityService,
        IAzureStorageDao storage)
    {
        this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    [FunctionName("Dashboards-Retrieve")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "dashboards")]
        HttpRequest req,
        CancellationToken cancellationToken) =>
        await req.UserRequest(cancellationToken, this.functionAuthenticator, async context =>
        {
            var dashboards = (await this.storage.DashboardsAsync(context.User.UserId, cancellationToken)).ToList();
            var entityUsers = await this.entityService.EntityUsersAsync(
                TableEntityType.Device, 
                dashboards.Select(d => d.RowKey), 
                cancellationToken);

            return dashboards.Select(p => new DashboardsDto(
                    p.RowKey,
                    p.Name,
                    p.ConfigurationSerialized,
                    entityUsers[p.RowKey].Select(u => new UserDto
                        {Id = u.RowKey, Email = u.Email, FullName = u.FullName}),
                    p.TimeStamp))
                .ToList();
        });

    private class DashboardsDto
    {
        public string Id { get; }

        public string Name { get; }

        public string? ConfigurationSerialized { get; }

        public DateTime? TimeStamp {  get; }

        public IEnumerable<UserDto> SharedWith { get; }


        public DashboardsDto(string id, string name, string? configurationSerialized, IEnumerable<UserDto> sharedWith, DateTime? timeStamp)
        {
            this.Id = id;
            this.Name = name;
            this.ConfigurationSerialized = configurationSerialized;
            this.SharedWith = sharedWith;
            this.TimeStamp = timeStamp;
        }
    }
}