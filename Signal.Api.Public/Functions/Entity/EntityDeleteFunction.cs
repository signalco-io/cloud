using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
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
using Signal.Core.Exceptions;
using Signal.Core.Sharing;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Entity;

public class EntityDeleteFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IEntityService entityService;
    private readonly ISharingService sharingService;

    public EntityDeleteFunction(
        IFunctionAuthenticator functionAuthenticator,
        IEntityService entityService,
        ISharingService sharingService)
    {
        this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        this.sharingService = sharingService ?? throw new ArgumentNullException(nameof(sharingService));
    }

    [FunctionName("Entity-Delete")]
    [OpenApiSecurityAuth0Token]
    [OpenApiOperation(nameof(EntityDeleteFunction), "Entity", Description = "Deletes the entity.")]
    [OpenApiRequestBody("application/json", typeof(EntityDeleteDto), Description = "Information about entity to delete.")]
    [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
    [OpenApiResponseBadRequestValidation]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "entity/delete")]
        HttpRequest req,
        CancellationToken cancellationToken) =>
        await req.UserRequest<EntityDeleteDto>(cancellationToken, this.functionAuthenticator, async context =>
        {
            if (string.IsNullOrWhiteSpace(context.Payload.Id))
                throw new ExpectedHttpException(HttpStatusCode.BadRequest, "Id property is required.");
            if (context.Payload.EntityType is null or TableEntityType.Unknown)
                throw new ExpectedHttpException(HttpStatusCode.BadRequest, "EntityType property is required.");

            await context.ValidateUserAssignedAsync(entityService, context.Payload.EntityType.Value, context.Payload.Id);
            
            // Remove device
            await this.entityService.RemoveByIdAsync(
                ItemTableNames.Devices, 
                context.Payload.Id, 
                cancellationToken);

            // Delete user assignments
            await this.sharingService.RemoveAssignmentsAsync(
                context.Payload.EntityType.Value, 
                context.Payload.Id,
                cancellationToken);
        });

    private class EntityDeleteDto
    {
        [Required]
        public string? Id { get; set; }

        [Required]
        public TableEntityType? EntityType { get; set; }
    }
}