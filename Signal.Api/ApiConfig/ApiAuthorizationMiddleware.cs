using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Signal.Infrastructure.ApiAuth.Oidc.Abstractions;

namespace Signal.Api.ApiConfig
{
    public class ApiAuthorizationMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IApiAuthorization apiAuthorization;
        private readonly ILogger<ApiAuthorizationMiddleware> logger;

        public ApiAuthorizationMiddleware(RequestDelegate next, IApiAuthorization apiAuthorization, ILogger<ApiAuthorizationMiddleware> logger)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.apiAuthorization = apiAuthorization ?? throw new ArgumentNullException(nameof(apiAuthorization));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var authorizationResult = await this.apiAuthorization.AuthorizeAsync(context.Request.Headers);
            if (authorizationResult.Failed)
            {
                this.logger.LogWarning(authorizationResult.FailureReason);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            // Call the next delegate/middleware in the pipeline
            await this.next(context);
        }
    }
}
