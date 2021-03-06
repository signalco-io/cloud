using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Signal.Api.Common.Auth;
using Signal.Api.Common.Conducts;
using Signal.Api.Common.Exceptions;
using Signal.Api.Common.OpenApi;
using Signal.Core.Entities;
using Signal.Core.Exceptions;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Conducts;

public class ConductRequestFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IEntityService entityService;
    private readonly IAzureStorageDao storageDao;

    public ConductRequestFunction(
        IFunctionAuthenticator functionAuthenticator,
        IEntityService entityService,
        IAzureStorageDao storageDao)
    {
        this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        this.storageDao = storageDao ?? throw new ArgumentNullException(nameof(storageDao));
    }

    [FunctionName("Conducts-Request")]
    [OpenApiSecurityAuth0Token]
    [OpenApiOperation(nameof(ConductRequestFunction), "Conducts", Description = "Requests conduct to be executed.")]
    [OpenApiRequestBody("application/json", typeof(ConductRequestDto), Description = "The conduct to execute.")]
    [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
    [OpenApiResponseBadRequestValidation]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "conducts/request")]
        HttpRequest req,
        [SignalR(HubName = "conducts")] IAsyncCollector<SignalRMessage> signalRMessages,
        CancellationToken cancellationToken) =>
        await req.UserRequest<ConductRequestDto>(cancellationToken, this.functionAuthenticator, async context =>
        {
            var payload = context.Payload;
            if (string.IsNullOrWhiteSpace(payload.EntityId) ||
                string.IsNullOrWhiteSpace(payload.ChannelName) ||
                string.IsNullOrWhiteSpace(payload.ContactName))
                throw new ExpectedHttpException(
                    HttpStatusCode.BadRequest,
                    "EntityId, ChannelName and ContactName properties are required.");

            await context.ValidateUserAssignedAsync(this.entityService, payload.EntityId);

            // TODO: Queue conduct on remote in case client doesn't receive signalR message

            // Retrieve all entity assigned users
            var entityUsers = (await this.storageDao.AssignedUsersAsync(
                new[] {payload.EntityId },
                cancellationToken)).FirstOrDefault();

            // Send to all users of the entity
            foreach (var entityUserId in entityUsers.Value)
            {
                await signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        Target = "requested",
                        Arguments = new object[] {JsonSerializer.Serialize(payload)},
                        UserId = entityUserId
                    }, cancellationToken);
            }
        });
}