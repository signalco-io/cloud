using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
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
using Signal.Api.Common.Conducts;
using Signal.Api.Common.Exceptions;
using Signal.Core.Entities;
using Signal.Core.Exceptions;
using Signal.Core.Notifications;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Conducts;

public class ConductRequestMultipleFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IEntityService entityService;
    private readonly IAzureStorageDao storageDao;
    private readonly INotificationService notificationService;

    public ConductRequestMultipleFunction(
        IFunctionAuthenticator functionAuthenticator,
        IEntityService entityService,
        IAzureStorageDao storageDao,
        INotificationService notificationService)
    {
        this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        this.storageDao = storageDao ?? throw new ArgumentNullException(nameof(storageDao));
        this.notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    [FunctionName("Conducts-RequestMultiple")]
    [OpenApiSecurityAuth0Token]
    [OpenApiOperation(nameof(ConductRequestMultipleFunction), "Conducts", Description = "Requests multiple conducts to be executed.")]
    [OpenApiRequestBody("application/json", typeof(List<ConductRequestMultipleDto>), Description = "Collection of conducts to execute.")]
    [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
    [OpenApiResponseBadRequestValidation]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "conducts/request-multiple")]
        HttpRequest req,
        [SignalR(HubName = "conducts")] IAsyncCollector<SignalRMessage> signalRMessages,
        CancellationToken cancellationToken) =>
        await req.UserRequest<List<ConductRequestMultipleDto>>(cancellationToken, this.functionAuthenticator, async context =>
        {
            var payload = context.Payload;
            var usersConducts = new Dictionary<string, ICollection<ConductRequestMultipleDto>>();
            foreach (var conduct in payload)
            {
                if (string.IsNullOrWhiteSpace(conduct.DeviceId) ||
                    string.IsNullOrWhiteSpace(conduct.ChannelName) ||
                    string.IsNullOrWhiteSpace(conduct.ContactName))
                    throw new ExpectedHttpException(
                        HttpStatusCode.BadRequest,
                        "DeviceId, ChannelName and ContactName properties are required.");

                if (conduct.DeviceId == "cloud") // TODO: Use channel discovery/router
                {
                    // Handle notification create conduct
                    if (conduct.ChannelName == "notification" && 
                        conduct.ContactName == "create" &&
                        !string.IsNullOrWhiteSpace(conduct.ValueSerialized))
                    {
                        var createRequest = JsonSerializer.Deserialize<ConductPayloadCloudNotificationCreate>(conduct.ValueSerialized);
                        if (createRequest is {Title: { }, Content: { }})
                        {
                            await this.notificationService.CreateAsync(
                                new[] {context.User.UserId},
                                new NotificationContent(
                                    createRequest.Title,
                                    createRequest.Content,
                                    NotificationContentType.Text),
                                new NotificationOptions(true),
                                cancellationToken);
                        }
                    }
                }
                else if (conduct.ChannelName == "slack") // TODO: Use channel discovery/router
                {
                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Authorization", req.Headers.Authorization[0]);
                    await client.PostAsync("https://slack.channel.api.signalco.io/api/conducts/request-multiple",
                        new StringContent(JsonSerializer.Serialize(new List<ConductRequestMultipleDto> {conduct}),
                            Encoding.UTF8, "application/json"), cancellationToken);
                }
                else
                {
                    var entityType = conduct.ChannelName == "station"
                        ? TableEntityType.Station
                        : TableEntityType.Device;

                    await context.ValidateUserAssignedAsync(this.entityService, entityType, conduct.DeviceId);

                    // Retrieve all device assigned devices
                    var deviceUsers = (await this.storageDao.AssignedUsersAsync(
                        entityType,
                        new[] {conduct.DeviceId},
                        cancellationToken)).FirstOrDefault();

                    foreach (var userId in deviceUsers.Value)
                        usersConducts.Append(userId, conduct);
                }
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
}