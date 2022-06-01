using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Signal.Api.Common;
using Signal.Api.Common.Exceptions;

namespace Signalco.Channel.Slack.Functions.Events;

public class SlackEventFunction
{
    private readonly ISlackRequestHandler slackRequestHandler;
    private readonly ILogger<SlackEventFunction> logger;

    public SlackEventFunction(
        ISlackRequestHandler slackRequestHandler,
        ILogger<SlackEventFunction> log)
    {
        this.slackRequestHandler = slackRequestHandler ?? throw new ArgumentNullException(nameof(slackRequestHandler));
        logger = log ?? throw new ArgumentNullException(nameof(log));
    }

    [FunctionName("Slack-Event")]
    [OpenApiOperation(nameof(SlackEventFunction), "Slack", "Event", Description = "Deletes the entity.")]
    [OpenApiRequestBody("application/json", typeof(EventRequestDto), Description = "Base model that provides content type information.")]
    [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
    [OpenApiResponseBadRequestValidation]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "hooks/event")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        await this.slackRequestHandler.VerifyFromSlack(req, cancellationToken);

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

    [Serializable]
    private class EventRequestDto
    {
        public string? Type { get; set; }
    }

    [Serializable]
    private class EventUrlVerificationRequestDto : EventRequestDto
    {
        public string? Challenge { get; set; }
    }
}