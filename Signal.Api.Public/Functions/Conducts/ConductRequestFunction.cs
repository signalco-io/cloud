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

namespace Signal.Api.Public.Functions.Conducts
{
    public class ConductRequestFunction
    {
        private readonly IFunctionAuthenticator functionAuthenticator;
        private readonly IAzureStorage storage;
        private readonly IAzureStorageDao storageDao;

        public ConductRequestFunction(
            IFunctionAuthenticator functionAuthenticator,
            IAzureStorage storage,
            IAzureStorageDao storageDao)
        {
            this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this.storageDao = storageDao ?? throw new ArgumentNullException(nameof(storageDao));
        }

        [FunctionName("Conducts-Request")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "conducts/request")]
            HttpRequest req,
            CancellationToken cancellationToken) =>
            await req.UserRequest<ConductRequestDto>(this.functionAuthenticator, async (user, payload) =>
            {
                if (string.IsNullOrWhiteSpace(payload.DeviceId) ||
                    string.IsNullOrWhiteSpace(payload.ChannelName) ||
                    string.IsNullOrWhiteSpace(payload.ContactName))
                    throw new ExpectedHttpException(
                        HttpStatusCode.BadRequest,
                        "DeviceId, ChannelName and ContactName properties are required.");
                
                // TODO: Queue conduct
            }, cancellationToken);

        private class ConductRequestDto
        {
            public string? DeviceId { get; set; }

            public string? ChannelName { get; set; }
            
            public string? ContactName { get; set; }

            public string? ValueSerialized { get; set; }
        }
    }
}