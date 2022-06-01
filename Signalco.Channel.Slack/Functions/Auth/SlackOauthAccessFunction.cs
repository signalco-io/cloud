using System;
using System.Net;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Signal.Api.Common;
using Signal.Api.Common.Auth;
using Signal.Api.Common.Exceptions;
using Signal.Core;
using Signal.Core.Devices;
using Signal.Core.Exceptions;
using Signal.Core.Storage;
using Signalco.Channel.Slack.Secrets;

namespace Signalco.Channel.Slack.Functions.Auth;

public class SlackOauthAccessFunction
{
    private readonly ISecretsManager secrets;
    private readonly ISlackRequestHandler slackRequestHandler;
    private readonly IFunctionAuthenticator authenticator;
    private readonly IEntityService entityService;
    private readonly IAzureStorage storage;

    public SlackOauthAccessFunction(
        ISecretsManager secrets,
        ISlackRequestHandler slackRequestHandler,
        IFunctionAuthenticator authenticator,
        IEntityService entityService,
        IAzureStorage storage)
    {
        this.secrets = secrets ?? throw new ArgumentNullException(nameof(secrets));
        this.slackRequestHandler = slackRequestHandler ?? throw new ArgumentNullException(nameof(slackRequestHandler));
        this.authenticator = authenticator ?? throw new ArgumentNullException(nameof(authenticator));
        this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    [FunctionName("Slack-Auth-OauthAccess")]
    [OpenApiOperation(nameof(SlackOauthAccessFunction), "Slack", "Auth", Description = "Creates new Slack channel.")]
    [OpenApiRequestBody("application/json", typeof(OAuthAccessRequestDto), Description = "OAuth code returned by Slack.")]
    [OpenApiOkJsonResponse(typeof(AccessResponseDto))]
    [OpenApiResponseBadRequestValidation]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/access")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        await this.slackRequestHandler.VerifyFromSlack(req, cancellationToken);
        return await req.UserRequest<OAuthAccessRequestDto, AccessResponseDto>(cancellationToken, authenticator, async context =>
        {
            // Resolve access token using temporary OAuth user code
            var clientId = await this.secrets.GetSecretAsync(SlackSecretKeys.ClientId, cancellationToken);
            var clientSecret = await this.secrets.GetSecretAsync(SlackSecretKeys.ClientSecret, cancellationToken);
            using var client = new HttpClient();
            var accessResponse = await client.PostAsJsonAsync("https://slack.com/api/oauth.v2.access", new
            {
                code = context.Payload.Code,
                client_id = clientId,
                client_secret = clientSecret
            }, cancellationToken);
            var access = await accessResponse.Content.ReadAsAsync<OAuthAccessResponseDto>(cancellationToken);
            if (!access.Ok)
                throw new ExpectedHttpException(HttpStatusCode.BadRequest, "Got not OK response from Slack, check your code and try again.");
            if (access.TokenType != "bot")
                throw new ExpectedHttpException(HttpStatusCode.BadRequest, $"Token type not supported: {access.TokenType}");

            // Persist to KeyVault with unique ID (generated)
            var accessSecretKey = Guid.NewGuid().ToString();
            await this.secrets.SetAsync(accessSecretKey, access.AccessToken, cancellationToken);

            // Create channel entity
            var channelId = await this.entityService.UpsertAsync(
                context.User.UserId,
                null,
                TableEntityType.Device,
                ItemTableNames.Devices,
                id => new DeviceTableEntity(id, "slack", "Slack", null, null, null),
                cancellationToken);

            // Create channel contact - auth token with ID
            // TODO: Use entity service
            await this.storage.CreateOrUpdateItemAsync(
                ItemTableNames.DeviceStates,
                new DeviceStateTableEntity(
                    channelId,
                    "slack",
                    "accessToken",
                    accessSecretKey,
                    DateTime.UtcNow),
                cancellationToken);

            return new AccessResponseDto(channelId);
        });
    }
    
    [Serializable]
    private class OAuthAccessRequestDto
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }
    }

    [Serializable]
    public class OAuthAccessResponseDto
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
    }

    [Serializable]
    private record AccessResponseDto(string id);
}