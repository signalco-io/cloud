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
using Microsoft.Extensions.Logging;
using Signal.Api.Common;
using Signal.Api.Common.Auth;
using Signal.Api.Common.Exceptions;
using Signal.Core;
using Signal.Core.Exceptions;
using Signalco.Channel.Slack.Secrets;

namespace Signalco.Channel.Slack.Functions.Auth;

public class SlackOauthAccessFunction
{
    private readonly ISecretsProvider secrets;
    private readonly ISlackRequestHandler slackRequestHandler;
    private readonly IFunctionAuthenticator authenticator;
    private readonly ILogger<SlackOauthAccessFunction> logger;

    public SlackOauthAccessFunction(
        ISecretsProvider secrets,
        ISlackRequestHandler slackRequestHandler,
        IFunctionAuthenticator authenticator,
        ILogger<SlackOauthAccessFunction> log)
    {
        this.secrets = secrets ?? throw new ArgumentNullException(nameof(secrets));
        this.slackRequestHandler = slackRequestHandler ?? throw new ArgumentNullException(nameof(slackRequestHandler));
        this.authenticator = authenticator ?? throw new ArgumentNullException(nameof(authenticator));
        logger = log ?? throw new ArgumentNullException(nameof(log));
    }

    [FunctionName("Slack-Auth-OauthAccess")]
    [OpenApiOperation(nameof(SlackOauthAccessFunction), "Slack", "Auth", Description = "Handles OAuth access.")]
    [OpenApiRequestBody("application/json", typeof(OAuthAccessRequestDto), Description = "Base model that provides content type information.")]
    [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
    [OpenApiResponseBadRequestValidation]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "hooks/event")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        await this.slackRequestHandler.VerifyFromSlack(req, cancellationToken);
        return await req.UserRequest<OAuthAccessRequestDto>(cancellationToken, authenticator, async context =>
        {
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

            // TODO: Persist to KeyVault with unique ID (generated)
            // TODO: Create channel entity
            // TODO: Create channel contact - auth token with ID
            // TODO: Return channel entity id (so we can redirect to instance)
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

        [JsonPropertyName("scope")]
        public string Scope { get; set; }
    }
}