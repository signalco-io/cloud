using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
using Signal.Core.Beacon;
using Signal.Core.Exceptions;
using Signal.Core.Sharing;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Beacons
{
    public class BeaconsRegisterFunction
    {
        private readonly IFunctionAuthenticator functionAuthenticator;
        private readonly IAzureStorage storage;
        private readonly IAzureStorageDao storageDao;
        private readonly ISharingService sharingService;

        public BeaconsRegisterFunction(
            IFunctionAuthenticator functionAuthenticator,
            IAzureStorage storage,
            IAzureStorageDao storageDao,
            ISharingService sharingService)
        {
            this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this.storageDao = storageDao ?? throw new ArgumentNullException(nameof(storageDao));
            this.sharingService = sharingService ?? throw new ArgumentNullException(nameof(sharingService));
        }

        [FunctionName("Beacons-Register")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "beacons/register")]
            HttpRequest req,
            CancellationToken cancellationToken) =>
            await req.UserRequest<BeaconRegisterRequestDto>(this.functionAuthenticator, async (user, payload) =>
            {
                if (payload.BeaconId == null)
                    throw new ExpectedHttpException(HttpStatusCode.BadRequest, "BeaconId is required.");

                // Check if beacons exists
                var existingBeacons = await this.storageDao.EntitiesByRowKeysAsync(
                    ItemTableNames.Beacons,
                    new[] { payload.BeaconId }, 
                    cancellationToken);
                if (existingBeacons.Any())
                    throw new ExpectedHttpException(HttpStatusCode.BadRequest, "Beacon already registered.");

                // Create or update existing item
                await this.storage.CreateOrUpdateItemAsync(ItemTableNames.Beacons,
                    new BeaconItem(user.UserId, payload.BeaconId)
                    {
                        RegisteredTimeStamp = DateTime.UtcNow
                    }, cancellationToken);

                // Assign to current user
                await this.sharingService.AssignToUserAsync(
                    user.UserId,
                    TableEntityType.Station,
                    payload.BeaconId,
                    cancellationToken);
            }, cancellationToken);

        private class BeaconRegisterRequestDto
        {
            [Required]
            public string? BeaconId { get; set; }
        }
    }
}