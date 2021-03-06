using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Signal.Api.Common.Auth;
using Signal.Api.Common.Conducts;
using Signal.Api.Common.Exceptions;
using Signal.Api.Common.OpenApi;
using Signal.Core.Entities;
using Signal.Core.Exceptions;
using Signal.Core.Storage;
using Signal.Core.Secrets;

namespace Signalco.Channel.Slack.Functions.Conducts;

public class ConductRequestMultipleFunction
{
    private readonly IFunctionAuthenticator authenticator;
    private readonly IAzureStorageDao storageDao;
    private readonly ISecretsProvider secrets;
    private readonly IEntityService entityService;
    private readonly ILogger<ConductRequestMultipleFunction> logger;

    public ConductRequestMultipleFunction(
        IFunctionAuthenticator authenticator,
        IAzureStorageDao storageDao,
        ISecretsProvider secrets,
        IEntityService entityService,
        ILogger<ConductRequestMultipleFunction> logger)
    {
        this.authenticator = authenticator ?? throw new ArgumentNullException(nameof(authenticator));
        this.storageDao = storageDao ?? throw new ArgumentNullException(nameof(storageDao));
        this.secrets = secrets ?? throw new ArgumentNullException(nameof(secrets));
        this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [FunctionName("Conducts-RequestMultiple")]
    [OpenApiSecurityAuth0Token]
    [OpenApiOperation(nameof(ConductRequestMultipleFunction), "Conducts",
        Description = "Requests multiple conducts to be executed.")]
    [OpenApiRequestBody("application/json", typeof(List<ConductRequestDto>),
        Description = "Collection of conducts to execute.")]
    [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
    [OpenApiResponseBadRequestValidation]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "conducts/request-multiple")]
        HttpRequest req,
        CancellationToken cancellationToken) =>
        await req.UserRequest<List<ConductRequestDto>>(cancellationToken, this.authenticator,
            async context =>
            {
                var payload = context.Payload;

                var anyErrors = false;
                foreach (var conductRequest in payload)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(conductRequest.EntityId))
                        {
                            this.logger.LogWarning("Entity not specified.");
                            continue;
                        }
                        if (conductRequest.ChannelName != KnownChannels.Slack)
                        {
                            this.logger.LogWarning("Not supported channel name {ChannelName}", conductRequest.ChannelName);
                            continue;
                        }

                        // Retrieve user channel with matching id
                        var matchingChannel = await this.entityService.GetAsync(conductRequest.EntityId, cancellationToken);
                        if (matchingChannel == null)
                        {
                            this.logger.LogWarning("Entity with id {EntityId} not found", conductRequest.EntityId);
                            continue;
                        }

                        // Resolve accessToken from channel contact via SecretsProvider                    
                        var contacts = await this.entityService.ContactsAsync(matchingChannel.Id, cancellationToken);
                        var accessTokenContact = contacts.FirstOrDefault(c => c.ContactName == KnownContacts.AccessToken);
                        if (accessTokenContact == null || string.IsNullOrWhiteSpace(accessTokenContact.ValueSerialized))
                        {
                            this.logger.LogWarning(
                                "Entity {EntityId} doesn't have assigned access token contact or access token value.",
                                matchingChannel.Id);
                            continue;
                        }
                        var accessToken = await this.secrets.GetSecretAsync(accessTokenContact.ValueSerialized, cancellationToken);
                        if (string.IsNullOrWhiteSpace(accessToken))
                        {
                            this.logger.LogWarning(
                                "Entity {EntityId} assigned access token is invalid.",
                                matchingChannel.Id);
                            continue;
                        }

                        // Execute action according to contact name
                        // TODO: Add support for conduct delay
                        if (conductRequest.ContactName == "sendMessage")
                        {
                            var sendMessagePayload = JsonSerializer.Deserialize<SendMessagePayload>(conductRequest.ValueSerialized ?? "");
                            
                            using var client = new HttpClient();
                            client.DefaultRequestHeaders.Authorization =
                                new AuthenticationHeaderValue("Bearer", accessToken);
                            await client.PostAsJsonAsync("https://slack.com/api/chat.postMessage", new
                            {
                                text = sendMessagePayload?.Text,
                                channel = sendMessagePayload?.ChannelId
                            }, cancellationToken);
                        }
                        else
                        {
                            throw new InvalidOperationException($"Unrecognized contact action: {conductRequest.ContactName}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, "Failed to execute conduct.");
                        anyErrors = true;
                    }
                }

                if (anyErrors)
                    throw new ExpectedHttpException(HttpStatusCode.InternalServerError,
                        "Some conducts failed to execute");
            });

    private class SendMessagePayload
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("channelId")]
        public string? ChannelId { get; set; }
    }
}