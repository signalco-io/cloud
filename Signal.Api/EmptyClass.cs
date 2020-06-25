using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Signal.Core;
using Signal.Infrastructure.ApiAuth.Oidc.Abstractions;
using Voyager.Api;

namespace Signal.Api
{
    [Voyager.Api.Route(HttpMethod.Get, "storage-table-list")]
    public class GetStorageTablesRequest : EndpointRequest<GetStorageTablesResponse>
    {
    }

    public class GetStorageTablesResponse
    {
        public IEnumerable<string> Items { get; set; }
    }

    public class GetStorageTablesHandler : EndpointHandler<GetStorageTablesRequest, GetStorageTablesResponse>
    {
        private readonly IAzureStorage azureStorage;

        public GetStorageTablesHandler(IAzureStorage azureStorage)
        {
            this.azureStorage = azureStorage ?? throw new System.ArgumentNullException(nameof(azureStorage));
        }

        public override async Task<ActionResult<GetStorageTablesResponse>> HandleRequestAsync(GetStorageTablesRequest request)
        {
            var items = await this.azureStorage.ListTables();
            return new GetStorageTablesResponse()
            {
                Items = items
            };
        }
    }

    public class GetVoyagerInfoResponse
    {
        public string Message { get; set; }
    }

    [Voyager.Api.Route(HttpMethod.Get, "voyager/info")]
    public class GetVoyagerInfoRequest : EndpointRequest<GetVoyagerInfoResponse>
    {
    }

    public class GetVoyagerInfoHandler : EndpointHandler<GetVoyagerInfoRequest, GetVoyagerInfoResponse>
    {
        public override ActionResult<GetVoyagerInfoResponse> HandleRequest(GetVoyagerInfoRequest request)
        {
            return new GetVoyagerInfoResponse { Message = "Voyager is awesome!" };
        }
    }

    public class Routes
    {
        private readonly HttpRouter router;

        public Routes(HttpRouter router)
        {
            this.router = router;
        }

        [FunctionName(nameof(FallbackRoute))]
        public Task<IActionResult> FallbackRoute([HttpTrigger(AuthorizationLevel.Anonymous, "get", "put", "delete", "post", "head", "trace", "patch", "connect", "options", Route = "{*path}")] HttpRequest req)
        {
            return router.Route(req.HttpContext);
        }
    }

    public class ApiAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IApiAuthorization apiAuthorization;
        private readonly ILogger<ApiAuthorizationMiddleware> logger;

        public ApiAuthorizationMiddleware(RequestDelegate next, IApiAuthorization apiAuthorization, ILogger<ApiAuthorizationMiddleware> logger)
        {
            _next = next ?? throw new System.ArgumentNullException(nameof(next));
            this.apiAuthorization = apiAuthorization ?? throw new System.ArgumentNullException(nameof(apiAuthorization));
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var authorizationResult = await apiAuthorization.AuthorizeAsync(context.Request.Headers);
            if (authorizationResult.Failed)
            {
                this.logger.LogWarning(authorizationResult.FailureReason);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }

    public static class ApiAuthorizationExtensions
    {
        public static IApplicationBuilder UseApiAuthorization(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiAuthorizationMiddleware>();
        }
    }
}
