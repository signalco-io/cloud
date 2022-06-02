using System;
using System.ComponentModel.DataAnnotations;
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
using Signal.Api.Common;
using Signal.Api.Common.Auth;
using Signal.Api.Common.Exceptions;
using Signal.Core;
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
            if (string.IsNullOrWhiteSpace(payload.DeviceId) ||
                string.IsNullOrWhiteSpace(payload.ChannelName) ||
                string.IsNullOrWhiteSpace(payload.ContactName))
                throw new ExpectedHttpException(
                    HttpStatusCode.BadRequest,
                    "DeviceId, ChannelName and ContactName properties are required.");

            var entityType = payload.ChannelName == "station" ? TableEntityType.Station : TableEntityType.Device;

            await context.ValidateUserAssignedAsync(this.entityService, entityType, payload.DeviceId);

            // TODO: Queue conduct on remote in case client doesn't receive signalR message

            // Retrieve all entity assigned users
            var deviceUsers = (await this.storageDao.AssignedUsersAsync(
                entityType,
                new[] {payload.DeviceId},
                cancellationToken)).FirstOrDefault();

            // Send to all users of the device
            foreach (var deviceUserId in deviceUsers.Value)
            {
                await signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        Target = "requested",
                        Arguments = new object[] {JsonSerializer.Serialize(payload)},
                        UserId = deviceUserId
                    }, cancellationToken);
            }
        });

    [Serializable]
    private class ConductRequestDto
    {
        [Required]
        public string? DeviceId { get; set; }

        [Required]
        public string? ChannelName { get; set; }

        [Required]
        public string? ContactName { get; set; }

        public string? ValueSerialized { get; set; }

        public double? Delay { get; set; }
    }
}