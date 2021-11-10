using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Signal.Api.Public.Auth;
using Signal.Api.Public.Exceptions;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Dashboards;

public class DashboardsRetrieveFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IAzureStorageDao storage;

    public DashboardsRetrieveFunction(
        IFunctionAuthenticator functionAuthenticator,
        IAzureStorageDao storage)
    {
        this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    [FunctionName("Dashboards-Retrieve")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "dashboards")]
        HttpRequest req,
        CancellationToken cancellationToken) =>
        await req.UserRequest(this.functionAuthenticator, async user =>
                (await this.storage.DashboardsAsync(user.UserId, cancellationToken))
                .Select(p => new DashboardsDto(
                    p.RowKey,
                    p.Name,
                    p.ConfigurationSerialized,
                    p.TimeStamp))
                .ToList(),
            cancellationToken);

    private class DashboardsDto
    {
        public string Id { get; }

        public string Name { get; }

        public string? ConfigurationSerialized { get; }

        public DateTime? TimeStamp {  get; }

        public DashboardsDto(string id, string name, string? configurationSerialized, DateTime? timeStamp)
        {
            this.Id = id;
            this.Name = name;
            this.ConfigurationSerialized = configurationSerialized;
            this.TimeStamp = timeStamp;
        }
    }
}