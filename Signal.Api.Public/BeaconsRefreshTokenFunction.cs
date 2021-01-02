using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Signal.Api.Public.Auth;

namespace Signal.Api.Public
{
    public class BeaconsRefreshTokenFunction
    {
        private readonly IFunctionAuthenticator functionAuthenticator;

        public BeaconsRefreshTokenFunction(
            IFunctionAuthenticator functionAuthenticator)
        {
            this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        }

        [FunctionName("Beacons-RefreshToken")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "beacons/refresh-token")]
            HttpRequest req,
            CancellationToken cancellationToken)
        {
            try
            {
                var request = await req.ReadAsJsonAsync<BeaconRefreshTokenRequestDto>();
                if (request == null || 
                    string.IsNullOrWhiteSpace(request.RefreshToken))
                    throw new ExpectedHttpException(HttpStatusCode.BadRequest, "\"RefreshToken\" property is required.");

                var token = await this.functionAuthenticator.RefreshTokenAsync(req, "", cancellationToken);
                return new OkObjectResult(token);
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