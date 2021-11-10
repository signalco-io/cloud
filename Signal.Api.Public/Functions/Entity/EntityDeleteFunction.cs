using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Signal.Api.Public.Auth;
using Signal.Api.Public.Exceptions;
using Signal.Core;
using Signal.Core.Exceptions;
using Signal.Core.Sharing;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Devices;

public class EntityDeleteFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IAzureStorage storage;
    private readonly IAzureStorageDao storageDao;
    private readonly IEntityService entityService;
    private readonly ISharingService sharingService;

    public EntityDeleteFunction(
        IFunctionAuthenticator functionAuthenticator,
        IAzureStorage storage,
        IAzureStorageDao storageDao,
        IEntityService entityService,
        ISharingService sharingService)
    {
        this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        this.storageDao = storageDao ?? throw new ArgumentNullException(nameof(storageDao));
        this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        this.sharingService = sharingService ?? throw new ArgumentNullException(nameof(sharingService));
    }

    [FunctionName("Entity-Delete")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "entity/delete")]
        HttpRequest req,
        CancellationToken cancellationToken) =>
        await req.UserRequest<EntityDeleteDto>(this.functionAuthenticator, async (user, payload) =>
        {
            if (string.IsNullOrWhiteSpace(payload.Id))
                throw new ExpectedHttpException(HttpStatusCode.BadRequest, "Id property is required.");
            if (payload.EntityType == null || payload.EntityType == TableEntityType.Unknown)
                throw new ExpectedHttpException(HttpStatusCode.BadRequest, "EntityType property is required.");

            // Check if entity is assigned to user
            var isOwned = await this.storageDao.IsUserAssignedAsync(
                user.UserId,
                payload.EntityType.Value,
                payload.Id,
                cancellationToken);
            if (!isOwned)
                throw new ExpectedHttpException(HttpStatusCode.NotFound);
                
            // Remove device
            await this.entityService.RemoveByIdAsync(
                ItemTableNames.Devices, 
                payload.Id, 
                cancellationToken);

            // Delete user assignments
            await this.sharingService.RemoveAssignmentsAsync(
                payload.EntityType.Value, 
                payload.Id,
                cancellationToken);
        }, cancellationToken);

    private class EntityDeleteDto
    {
        [Required]
        public string? Id { get; set; }

        [Required]
        public TableEntityType? EntityType { get; set; }
    }
}