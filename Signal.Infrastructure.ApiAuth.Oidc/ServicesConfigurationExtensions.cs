using Microsoft.Extensions.DependencyInjection;
using Signal.Infrastructure.ApiAuth.Oidc.Abstractions;

namespace Signal.Infrastructure.ApiAuth.Oidc
{
    public static class ServicesConfigurationExtensions
    {
        public static void AddApiAuthOidc(this IServiceCollection services)
        {
            // These are created as a singletons, so that only one instance of each
            // is created for the lifetime of the hosting Azure Function App.
            // That helps reduce the number of calls to the authorization service
            // for the signing keys and other stuff that can be used across multiple
            // calls to the HTTP triggered Azure Functions.

            services.AddSingleton<IAuthorizationHeaderBearerTokenExtractor, AuthorizationHeaderBearerTokenExtractor>();
            services.AddSingleton<IJwtSecurityTokenHandlerWrapper, JwtSecurityTokenHandlerWrapper>();
            services.AddSingleton<IOidcConfigurationManager, OidcConfigurationManager>();

            services.AddSingleton<IApiAuthorization, OidcApiAuthorizationService>();
        }
    }
}
