using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Signal.Api.Common;
using Signal.Api.Common.Auth;
using Signal.Api.Common.Exceptions;

namespace Signal.Api.Public.Functions.SignalR;

public class DevicesNegotiateFunction
{
    private readonly IFunctionAuthenticator authenticator;

    public DevicesNegotiateFunction(
        IFunctionAuthenticator authenticator)
    {
        this.authenticator = authenticator ?? throw new ArgumentNullException(nameof(authenticator));
    }

    [FunctionName("SignalR-Devices-Negotiate")]
    [OpenApiSecurityAuth0Token]
    [OpenApiOperation(operationId: nameof(DevicesNegotiateFunction), tags: new[] { "SignalR" },
        Description = "Negotiates SignalR connection for devices hub.")]
    [OpenApiOkJsonResponse(typeof(SignalRConnectionInfo), Description = "SignalR connection info.")]
    public async Task<IActionResult> Negotiate(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "signalr/devices/negotiate")]
        HttpRequest req,
        IBinder binder, CancellationToken cancellationToken) =>
        // This style is an example of imperative attribute binding; the mechanism for declarative binding described below does not work
        // UserId = "{headers.x-my-custom-header}" https://docs.microsoft.com/en-us/azure/azure-signalr/signalr-concept-serverless-development-config
        // Source: https://charliedigital.com/2019/09/02/azure-functions-signalr-and-authorization/
        await req.UserRequest(cancellationToken, this.authenticator, async context =>
            await binder.BindAsync<SignalRConnectionInfo>(new SignalRConnectionInfoAttribute
            {
                HubName = "devices",
                UserId = context.User.UserId
            }, cancellationToken));
}