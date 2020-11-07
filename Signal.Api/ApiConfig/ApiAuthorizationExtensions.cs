using Microsoft.AspNetCore.Builder;

namespace Signal.Api.ApiConfig
{
    public static class ApiExtensions
    {
        public static IApplicationBuilder UseApiAuthorization(this IApplicationBuilder builder) =>
            builder.UseMiddleware<ApiAuthorizationMiddleware>();
    }
}
