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
using Signal.Core.Beacon;
using Signal.Core.Exceptions;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Beacons
{
    public class BeaconsReportStateFunction
    {
        private readonly IFunctionAuthenticator functionAuthenticator;
        private readonly IAzureStorage storage;

        public BeaconsReportStateFunction(
            IFunctionAuthenticator functionAuthenticator,
            IAzureStorage storage)
        {
            this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        [FunctionName("Beacons-ReportState")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "beacons/report-state")]
            HttpRequest req,
            CancellationToken cancellationToken) =>
            await req.UserRequest<BeaconReportStateRequestDto>(this.functionAuthenticator, async (user, payload) =>
            {
                if (payload.Id == null)
                    throw new ExpectedHttpException(HttpStatusCode.BadRequest, "BeaconId is required.");

                // TODO: Check if beacons exists

                await this.storage.UpdateItemAsync(
                    ItemTableNames.Beacons,
                    new BeaconStateItem(user.UserId, payload.Id)
                    {
                        StateTimeStamp = DateTime.UtcNow,
                        Version = payload.Version
                    }, cancellationToken);
            }, cancellationToken);

        private class BeaconReportStateRequestDto
        {
            [Required]
            public string? Id { get; set; }

            [Required]
            public string? Version { get; set; }
        }
    }
}