using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Signal.Api.Common.Auth;
using Signal.Api.Common.Exceptions;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Beacons;

public class BeaconsRetrieveFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IAzureStorageDao storageDao;

    public BeaconsRetrieveFunction(
        IFunctionAuthenticator functionAuthenticator,
        IAzureStorageDao storage)
    {
        this.functionAuthenticator =
            functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.storageDao = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    [FunctionName("Beacons-Retrieve")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "beacons")]
        HttpRequest req,
        CancellationToken cancellationToken) =>
        await req.UserRequest<IEnumerable<StationDto>>(cancellationToken, this.functionAuthenticator, async context =>
                (await this.storageDao.BeaconsAsync(context.User.UserId, cancellationToken))
                .Select(b => new StationDto(b.RowKey)
                {
                    Version = b.Version,
                    StateTimeStamp = b.StateTimeStamp,
                    RegisteredTimeStamp = b.RegisteredTimeStamp,
                    AvailableWorkerServices = b.AvailableWorkerServices,
                    RunningWorkerServices = b.RunningWorkerServices
                })
                .ToList());
        
    [Serializable]
    private class StationDto
    {
        public StationDto(string id)
        {
            Id = id;
        }

        public string Id { get; }

        public string? Version { get; set; }

        public DateTime? StateTimeStamp { get; set; }

        public DateTime RegisteredTimeStamp { get; set; }
         
        public IEnumerable<string>? AvailableWorkerServices { get; set; }

        public IEnumerable<string>? RunningWorkerServices { get; set; }
    }
}