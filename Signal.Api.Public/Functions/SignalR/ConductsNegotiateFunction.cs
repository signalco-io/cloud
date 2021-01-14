using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Signal.Api.Public.Auth;
using Signal.Api.Public.Exceptions;

namespace Signal.Api.Public.Functions.SignalR
{
    public class ConductsNegotiateFunction
    {
        private readonly IFunctionAuthenticator authenticator;

        public ConductsNegotiateFunction(
            IFunctionAuthenticator authenticator)
        {
            this.authenticator = authenticator ?? throw new ArgumentNullException(nameof(authenticator));
        }

        [FunctionName("SignalR-Conducts-Negotiate")]
        public async Task<IActionResult> Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "signalr/conducts/negotiate")]
            HttpRequest req,
            IBinder binder, CancellationToken cancellationToken) =>
            // This style is an example of imperative attribute binding; the mechanism for declarative binding described below does not work
            // UserId = "{headers.x-my-custom-header}" https://docs.microsoft.com/en-us/azure/azure-signalr/signalr-concept-serverless-development-config
            // Source: https://charliedigital.com/2019/09/02/azure-functions-signalr-and-authorization/
            await req.UserRequest(this.authenticator, async user =>
                await binder.BindAsync<SignalRConnectionInfo>(new SignalRConnectionInfoAttribute
                {
                    HubName = "conducts",
                    UserId = user.UserId
                }, cancellationToken), cancellationToken);
    }
}