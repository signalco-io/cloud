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
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.IdentityModel.Tokens;
using Signal.Api.Public.Auth;
using Signal.Api.Public.Exceptions;
using Signal.Core.Exceptions;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Conducts
{
    public class ConductRequestFunction
    {
        private readonly IFunctionAuthenticator functionAuthenticator;
        private readonly IAzureStorageDao storageDao;

        public ConductRequestFunction(
            IFunctionAuthenticator functionAuthenticator,
            IAzureStorageDao storageDao)
        {
            this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
            this.storageDao = storageDao ?? throw new ArgumentNullException(nameof(storageDao));
        }

        [FunctionName("Conducts-Request")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "conducts/request")]
            HttpRequest req,
            [SignalR(HubName = "conducts")] IAsyncCollector<SignalRMessage> signalRMessages,
            CancellationToken cancellationToken) =>
            await req.UserRequest<ConductRequestDto>(this.functionAuthenticator, async (user, payload) =>
            {
                if (string.IsNullOrWhiteSpace(payload.DeviceId) ||
                    string.IsNullOrWhiteSpace(payload.ChannelName) ||
                    string.IsNullOrWhiteSpace(payload.ContactName))
                    throw new ExpectedHttpException(
                        HttpStatusCode.BadRequest,
                        "DeviceId, ChannelName and ContactName properties are required.");

                var entityType = payload.ChannelName == "station" ? TableEntityType.Station : TableEntityType.Device;
                
                // Check if user has assigned device
                await this.AssertEntityAssigned(
                    user.UserId, entityType, payload.DeviceId,
                    cancellationToken);

                // TODO: Queue conduct on remote in case client doesn't receive signalR message

                // Retrieve all device assigned devices
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
            }, cancellationToken);

        private async Task AssertEntityAssigned(string userId, TableEntityType entityType, string entityId, CancellationToken cancellationToken)
        {
            if (!(await this.storageDao.IsUserAssignedAsync(
                userId, entityType, entityId, cancellationToken)))
                throw new ExpectedHttpException(HttpStatusCode.NotFound);
        }

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
}