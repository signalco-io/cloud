using System;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Text;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Signal.Api.Common;
using Signal.Api.Public.Auth;
using Signal.Api.Public.Exceptions;
using Signal.Core;
using Signal.Core.Exceptions;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Beacons;

public class StationsLoggingDownloadFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IEntityService entityService;
    private readonly IAzureStorageDao azureStorageDao;
    private readonly ILogger<StationsLoggingDownloadFunction> logger;

    public StationsLoggingDownloadFunction(
        IFunctionAuthenticator functionAuthenticator,
        IEntityService entityService,
        IAzureStorageDao azureStorageDao,
        ILogger<StationsLoggingDownloadFunction> logger)
    {
        this.functionAuthenticator =
            functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        this.azureStorageDao = azureStorageDao ?? throw new ArgumentNullException(nameof(azureStorageDao));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [FunctionName("Stations-Logging-Download")]
    [OpenApiSecurityAuth0Token]
    [OpenApiOperation(nameof(StationsLoggingDownloadFunction), "Stations")]
    [OpenApiParameter("stationId", In = ParameterLocation.Query, Required = true, Type = typeof(string),
        Description = "The **stationId** parameter")]
    [OpenApiParameter("blobName", In = ParameterLocation.Query, Required = true, Type = typeof(string),
        Description = "The **blobName** parameter. Use list function to obtain available blobs.")]
    [OpenApiResponseBadRequestValidation]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stations/logging/download")]
        HttpRequest req,
        CancellationToken cancellationToken)
    {
        try
        {
            return await req.UserRequest(cancellationToken, this.functionAuthenticator, async context =>
            {
                string stationId = req.Query["stationId"];
                if (string.IsNullOrWhiteSpace(stationId))
                    throw new ExpectedHttpException(HttpStatusCode.BadRequest, "stationId is required");
                string blobName = req.Query["blobName"];
                if (string.IsNullOrWhiteSpace(blobName))
                    throw new ExpectedHttpException(HttpStatusCode.BadRequest, "blobName is required");

                await context.ValidateUserAssignedAsync(
                    this.entityService,
                    TableEntityType.Station,
                    stationId);

                var stream = await this.azureStorageDao.LoggingDownloadAsync(blobName, cancellationToken);
                var content = Encoding.UTF8.GetBytes(await new StreamReader(stream).ReadToEndAsync());

                return new FileContentResult(content, "text/plain");
            });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed");
            throw;
        }
    }
}