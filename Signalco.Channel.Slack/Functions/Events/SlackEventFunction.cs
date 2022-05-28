using System;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Signal.Api.Common;
using Signal.Api.Common.Exceptions;
using Signal.Core;
using Signal.Core.Exceptions;
using Signalco.Channel.Slack.Secrets;

namespace Signalco.Channel.Slack.Functions.Events
{
    public class SlackEventFunction
    {
        private readonly ISecretsProvider secrets;
        private readonly ILogger<SlackEventFunction> logger;

        public SlackEventFunction(
            ISecretsProvider secrets,
            ILogger<SlackEventFunction> log)
        {
            this.secrets = secrets ?? throw new ArgumentNullException(nameof(secrets));
            logger = log ?? throw new ArgumentNullException(nameof(log));
        }

        [FunctionName("Slack-Event")]
        [OpenApiOperation(nameof(SlackEventFunction), "Slack", "Event", Description = "Deletes the entity.")]
        [OpenApiRequestBody("application/json", typeof(EventRequestDto), Description = "Base model that provides content type information.")]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        [OpenApiResponseBadRequestValidation]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "hooks/event")] HttpRequest req)
        {
            await VerifyFromSlack(req);

            var eventReq = await req.ReadAsJsonAsync<EventRequestDto>();
            switch (eventReq.Type)
            {
                case "url_verification":
                    var verifyRequest = await req.ReadAsJsonAsync<EventUrlVerificationRequestDto>();
                    return new OkObjectResult(new
                    {
                        challenge = verifyRequest.Challenge
                    });
                default:
                    this.logger.LogWarning("Unknown event request type {Type}", eventReq.Type);
                    return new BadRequestResult();
            }
        }

        private async Task VerifyFromSlack(HttpRequest req)
        {
            var signature = req.Headers["X-Slack-Signature"];
            var timeStamp = req.Headers["X-Slack-Request-Timestamp"];
            var signingSecret = await this.secrets.GetSecretAsync(SlackSecretKeys.SigningSecret);
            var content = await req.ReadAsStringAsync();

            var signBaseString = $"v0:{timeStamp}:{content}";

            var encoding = new UTF8Encoding();
            using var hmac = new HMACSHA256(encoding.GetBytes(signingSecret));
            var hash = hmac.ComputeHash(encoding.GetBytes(signBaseString));
            var hashString = $"v0={BitConverter.ToString(hash).Replace("-", "").ToLower(CultureInfo.InvariantCulture)}";

            if (hashString != signature)
            {
                this.logger.LogWarning("Slack signature not matching content");
                throw new ExpectedHttpException(HttpStatusCode.BadRequest, "Signature not valid");
            }
        }

        [Serializable]
        private class EventRequestDto
        {
            public string? Type { get; set; }
        }

        [Serializable]
        private class EventUrlVerificationRequestDto : EventRequestDto
        {
            public string? Token { get; set; }

            public string? Challenge { get; set; }
        }
    }
}

