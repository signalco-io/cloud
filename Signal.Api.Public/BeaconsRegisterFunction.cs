using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Signal.Api.Public.Auth;
using Signal.Core;

namespace Signal.Api.Public
{
    public class BeaconsRegisterFunction
    {
        private readonly IFunctionAuthenticator functionAuthenticator;
        private readonly IAzureStorage storage;

        public BeaconsRegisterFunction(
            IFunctionAuthenticator functionAuthenticator,
            IAzureStorage storage)
        {
            this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        [FunctionName("Beacons-Register")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "beacons/register")]
            HttpRequest req,
            CancellationToken cancellationToken)
        {
            try
            {
                var user = await this.functionAuthenticator.AuthenticateAsync(req, cancellationToken);

                var registerRequest = await req.ReadAsJsonAsync<BeaconRegisterRequestDto>();

                if (registerRequest == null)
                    throw new ExpectedHttpException(HttpStatusCode.BadRequest, "Unable to deserialize request.");
                if (registerRequest.BeaconId == null)
                    throw new ExpectedHttpException(HttpStatusCode.BadRequest, "BeaconId is null.");
                
                // TODO: Check if beacons exists
                
                await this.storage.CreateOrUpdateItemAsync(ItemTableNames.Beacons, new BeaconItem(user.UserId, registerRequest.BeaconId)
                {
                    RegisteredTimeStamp = DateTime.UtcNow
                }, cancellationToken);

                return new OkResult();
            }
            catch (ExpectedHttpException ex)
            {
                return new ObjectResult(new ApiErrorDto(ex.Code.ToString(), ex.Message))
                {
                    StatusCode = (int)ex.Code
                };
            }
        }
    }
}