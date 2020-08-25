using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Signal.Infrastructure.ApiAuth.Oidc.Abstractions;

namespace Signal.Api.ApiConfig
{
    public class ApiCorsMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiCorsMiddleware(RequestDelegate next, IApiAuthorization apiAuthorization, ILogger<ApiAuthorizationMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var req = context.Request;
            if (!req.HttpContext.Response.Headers.ContainsKey("Access-Control-Allow-Credentials"))
            {
                req.HttpContext.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            }
            if (!req.HttpContext.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
            {
                req.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            }
            if (!req.Headers.ContainsKey("Access-Control-Request-Headers"))
            {
                req.HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", req.Headers["access-control-request-headers"][0]);
            }
            await _next(context);
        }
    }
}
