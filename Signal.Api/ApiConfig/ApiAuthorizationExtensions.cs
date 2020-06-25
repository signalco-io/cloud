using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Signal.Core;

namespace Signal.Api.ApiConfig
{
    public static class ApiAuthorizationExtensions
    {
        public static IApplicationBuilder UseApiAuthorization(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiAuthorizationMiddleware>();
        }
    }
}
