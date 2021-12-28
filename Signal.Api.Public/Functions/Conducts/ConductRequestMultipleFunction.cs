using System;
using System.Collections.Generic;
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
using Signal.Api.Common;
using Signal.Api.Public.Auth;
using Signal.Api.Public.Exceptions;
using Signal.Core;
using Signal.Core.Exceptions;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Conducts;

public class ConductRequestMultipleFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IEntityService entityService;
    private readonly IAzureStorageDao storageDao;

    public ConductRequestMultipleFunction(
        IFunctionAuthenticator functionAuthenticator,
        IEntityService entityService,
        IAzureStorageDao storageDao)
    {
        this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        this.storageDao = storageDao ?? throw new ArgumentNullException(nameof(storageDao));
    }

    [FunctionName("Conducts-RequestMultiple")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "conducts/request-multiple")]
        HttpRequest req,
        [SignalR(HubName = "conducts")] IAsyncCollector<SignalRMessage> signalRMessages,
        CancellationToken cancellationToken) =>
        await req.UserRequest<List<ConductRequestDto>>(cancellationToken, this.functionAuthenticator, async context =>
        {
            var payload = context.Payload;
            var usersConducts = new Dictionary<string, ICollection<ConductRequestDto>>();
            foreach (var conduct in payload)
            {
                if (string.IsNullOrWhiteSpace(conduct.DeviceId) ||
                    string.IsNullOrWhiteSpace(conduct.ChannelName) ||
                    string.IsNullOrWhiteSpace(conduct.ContactName))
                    throw new ExpectedHttpException(
                        HttpStatusCode.BadRequest,
                        "DeviceId, ChannelName and ContactName properties are required.");

                var entityType = conduct.ChannelName == "station" ? TableEntityType.Station : TableEntityType.Device;

                await context.ValidateUserAssignedAsync(this.entityService, entityType, conduct.DeviceId);

                // Retrieve all device assigned devices
                var deviceUsers = (await this.storageDao.AssignedUsersAsync(
                    entityType,
                    new[] { conduct.DeviceId },
                    cancellationToken)).FirstOrDefault();

                foreach (var userId in deviceUsers.Value) 
                    usersConducts.Append(userId, conduct);
            }

            // TODO: Queue conduct on remote in case client doesn't receive signalR message

            // Send to all users of the device
            foreach (var userId in usersConducts.Keys)
            {
                var conducts = usersConducts[userId];
                await signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        Target = "requested-multiple",
                        Arguments = new object[] { JsonSerializer.Serialize(conducts) },
                        UserId = userId
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