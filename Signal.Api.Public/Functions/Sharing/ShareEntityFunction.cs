using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
using Signal.Api.Public.Auth;
using Signal.Api.Public.Exceptions;
using Signal.Core;
using Signal.Core.Exceptions;
using Signal.Core.Sharing;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Sharing;

public class ShareEntityFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly ISharingService sharingService;
    private readonly IEntityService entityService;
    private readonly ILogger<ShareEntityFunction> logger;

    public ShareEntityFunction(
        IFunctionAuthenticator functionAuthenticator,
        ISharingService sharingService,
        IEntityService entityService,
        ILogger<ShareEntityFunction> logger)
    {
        this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.sharingService = sharingService ?? throw new ArgumentNullException(nameof(sharingService));
        this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [FunctionName("Share-Entity")]
    [OpenApiSecurityAuth0Token]
    [OpenApiOperation(nameof(ShareEntityFunction), "Sharing", Description = "Shared the entity with other users.")]
    [OpenApiRequestBody("application/json", typeof(ShareRequestDto), Description = "Share one entity with one or more users.")]
    [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
    [OpenApiResponseBadRequestValidation]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "share/entity")]
        HttpRequest req,
        CancellationToken cancellationToken) =>
        await req.UserRequest<ShareRequestDto>(cancellationToken, this.functionAuthenticator,
            async context =>
            {
                if (context.Payload.Type == null)
                    throw new ExpectedHttpException(HttpStatusCode.BadRequest, "Type is required");
                if (string.IsNullOrWhiteSpace(context.Payload.EntityId))
                    throw new ExpectedHttpException(HttpStatusCode.BadRequest, "EntityId is required");
                if (context.Payload.UserEmails == null || !context.Payload.UserEmails.Any())
                    throw new ExpectedHttpException(HttpStatusCode.BadRequest, "UserEmails is required - at least one user email is required");

                await context.ValidateUserAssignedAsync(this.entityService, context.Payload.Type.Value, context.Payload.EntityId);
                
                foreach (var userEmail in context.Payload.UserEmails.Where(userEmail => !string.IsNullOrWhiteSpace(userEmail)))
                {
                    try
                    {
                        await this.sharingService.AssignToUserEmailAsync(
                            userEmail,
                            context.Payload.Type.Value, context.Payload.EntityId,
                            cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogInformation(ex, "Failed to share with provided user.");
                    }
                }
            });

    private class ShareRequestDto
    {
        [Required]
        public TableEntityType? Type { get; set; }

        [Required]
        public string? EntityId { get; set; }

        [Required]
        public List<string>? UserEmails { get; set; }
    }
}