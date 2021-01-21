using System;
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

namespace Signal.Api.Public.Functions.Processes
{
    public class ProcessesSetFunction
    {
        private readonly IFunctionAuthenticator functionAuthenticator;
        private readonly IAzureStorage storage;

        public ProcessesSetFunction(
            IFunctionAuthenticator functionAuthenticator,
            IAzureStorage storage)
        {
            this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        [FunctionName("Processes-Set")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "processes/set")]
            HttpRequest req,
            CancellationToken cancellationToken) =>
            await req.UserRequest<ProcessSetDto, ProcessSetResponseDto>(this.functionAuthenticator,
                async (user, payload) =>
                {
                    if (string.IsNullOrWhiteSpace(payload.Alias))
                        throw new ExpectedHttpException(HttpStatusCode.BadRequest, "Alias is required");

                    var id = payload.Id ?? Guid.NewGuid().ToString();
                    await this.storage.CreateOrUpdateItemAsync(
                        ItemTableNames.Processes,
                        new ProcessTableEntity(
                            ProcessType.StateTriggered.ToString(),
                            id,
                            payload.Alias,
                            payload.IsDisabled ?? false,
                            payload.ConfigurationSerialized),
                        cancellationToken);
                    return new ProcessSetResponseDto(id);
                }, cancellationToken);

        private class ProcessSetDto
        {
            public string? Id { get; set;  }

            public string? Alias { get; set; }

            public bool? IsDisabled { get; set;  }

            public string? ConfigurationSerialized { get; set; }
        }

        private class ProcessSetResponseDto
        {
            public string Id { get; }

            public ProcessSetResponseDto(string id)
            {
                this.Id = id;
            }
        }
    }
}