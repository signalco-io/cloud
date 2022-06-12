using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Signal.Api.Common.Auth;
using Signal.Api.Common.Exceptions;
using Signal.Core.Entities;
using Signal.Core.Exceptions;
using Signal.Core.Sharing;

namespace Signal.Api.Public.Functions.Beacons;

public class BeaconsRegisterFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IEntityService entityService;
    private readonly ISharingService sharingService;

    public BeaconsRegisterFunction(
        IFunctionAuthenticator functionAuthenticator,
        IEntityService entityService,
        ISharingService sharingService)
    {
        this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        this.sharingService = sharingService ?? throw new ArgumentNullException(nameof(sharingService));
    }

    [FunctionName("Beacons-Register")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "beacons/register")]
        HttpRequest req,
        CancellationToken cancellationToken) =>
        await req.UserRequest<BeaconRegisterRequestDto>(cancellationToken, this.functionAuthenticator, async context =>
        {
            var payload = context.Payload;
            var user = context.User;
            if (payload.BeaconId == null)
                throw new ExpectedHttpException(HttpStatusCode.BadRequest, "BeaconId is required.");
            
            // Create or update existing item
            await this.entityService.UpsertAsync(
                user.UserId,
                payload.BeaconId,
                id => new Core.Entities.Entity(EntityType.Station, id, "Station"), 
                cancellationToken);

            // Assign to current user
            await this.sharingService.AssignToUserAsync(
                user.UserId,
                payload.BeaconId,
                cancellationToken);
        });

    [Serializable]
    private class BeaconRegisterRequestDto
    {
        [Required]
        public string? BeaconId { get; set; }
    }
}