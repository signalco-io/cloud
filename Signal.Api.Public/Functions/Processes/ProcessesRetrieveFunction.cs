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
using Signal.Core;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Processes;

public class ProcessesRetrieveFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IAzureStorageDao storage;

    public ProcessesRetrieveFunction(
        IFunctionAuthenticator functionAuthenticator,
        IAzureStorageDao storage)
    {
        this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    [FunctionName("Processes-Retrieve")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "processes")]
        HttpRequest req,
        CancellationToken cancellationToken) =>
        await req.UserRequest(cancellationToken, this.functionAuthenticator, async context =>
                (await this.storage.ProcessesAsync(context.User.UserId, cancellationToken))
                .Select(p => new ProcessDto(
                    p.PartitionKey,
                    p.RowKey,
                    p.Alias,
                    p.IsDisabled,
                    p.ConfigurationSerialized))
                .ToList());

    private class ProcessDto
    {
        public string Type { get; }
            
        public string Id { get; }
            
        public string Alias { get; }
            
        public bool IsDisabled { get; }
            
        public string? ConfigurationSerialized { get; }

        public ProcessDto(string type, string id, string alias, bool isDisabled, string? configurationSerialized)
        {
            this.Type = type;
            this.Id = id;
            this.Alias = alias;
            this.IsDisabled = isDisabled;
            this.ConfigurationSerialized = configurationSerialized;
        }
    }
}