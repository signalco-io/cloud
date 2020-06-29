using Microsoft.AspNetCore.Builder;

namespace Signal.Api.ApiConfig
{
    public static class ApiExtensions
    {
        public static IApplicationBuilder UseApiCors(this IApplicationBuilder builder) =>
            builder.UseMiddleware<ApiCorsMiddleware>();

        public static IApplicationBuilder UseApiAuthorization(this IApplicationBuilder builder) =>
            builder.UseMiddleware<ApiAuthorizationMiddleware>();
    }
}
