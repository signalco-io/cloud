using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Signal.Core;
using Signal.Infrastructure.ApiAuth.Oidc.Abstractions;
using Signal.Infrastructure.ApiAuth.Oidc.Models;

namespace Signal.Infrastructure.ApiAuth.Oidc
{
    /// <summary>
    /// Encapsulates checks of OpenID Connect (OIDC) Authorization tokens in HTTP request headers.
    /// </summary>
    public class OidcApiAuthorizationService : IApiAuthorization
    {
        private readonly IAuthorizationHeaderBearerTokenExtractor _authorizationHeaderBearerTokenExractor;

        private readonly IJwtSecurityTokenHandlerWrapper _jwtSecurityTokenHandlerWrapper;

        private readonly IOidcConfigurationManager _oidcConfigurationManager;
        private readonly ISecretsProvider secretsProvider;

        private string issuer;
        private string audience;

        public OidcApiAuthorizationService(
            IAuthorizationHeaderBearerTokenExtractor authorizationHeaderBearerTokenExractor,
            IJwtSecurityTokenHandlerWrapper jwtSecurityTokenHandlerWrapper,
            IOidcConfigurationManager oidcConfigurationManager,
            ISecretsProvider secretsProvider)
        {
            _authorizationHeaderBearerTokenExractor = authorizationHeaderBearerTokenExractor;
            _jwtSecurityTokenHandlerWrapper = jwtSecurityTokenHandlerWrapper;
            _oidcConfigurationManager = oidcConfigurationManager;
            this.secretsProvider = secretsProvider ?? throw new ArgumentNullException(nameof(secretsProvider));
        }

        private async Task<string> IssuerAsync()
        {
            if (this.issuer != null) return this.issuer;

            this.issuer = await this.secretsProvider.GetSecretAsync(SecretKeys.OidcApiAuthorizationSettings.IssuerUrl);
            return this.issuer;
        }

        private async Task<string> AudienceAsync()
        {
            if (this.audience != null) return this.audience;

            this.audience = await this.secretsProvider.GetSecretAsync(SecretKeys.OidcApiAuthorizationSettings.Audience);
            return this.audience;
        }

        /// <summary>
        /// Checks the given HTTP request headers for a valid OpenID Connect (OIDC) Authorization token.
        /// </summary>
        /// <param name="httpRequestHeaders">
        /// The HTTP request headers to check.
        /// </param>
        /// <returns>
        /// Informatoin about the success or failure of the authorization.
        /// </returns>
        public async Task<ApiAuthorizationResult> AuthorizeAsync(
            IHeaderDictionary httpRequestHeaders)
        {
            string authorizationBearerToken = _authorizationHeaderBearerTokenExractor.GetToken(
                httpRequestHeaders);
            if (authorizationBearerToken == null)
            {
                return new ApiAuthorizationResult(
                    "Authorization header is missing, invalid format, or is not a Bearer token.");
            }

            bool isTokenValid = false;

            int validationRetryCount = 0;

            do
            {
                IEnumerable<SecurityKey> isserSigningKeys;
                try
                {
                    // Get the cached signing keys if they were retrieved previously. 
                    // If they haven't been retrieved, or the cached keys are stale,
                    // then a fresh set of signing keys are retrieved from the OpenID Connect provider
                    // (issuer) cached and returned.
                    // This method will throw if the configuration cannot be retrieved, instead of returning null.
                    isserSigningKeys = await _oidcConfigurationManager.GetIssuerSigningKeysAsync();
                }
                catch (Exception ex)
                {
                    return new ApiAuthorizationResult(
                        "Problem getting signing keys from Open ID Connect provider (issuer)."
                        + $" ConfigurationManager threw {ex.GetType()} Message: {ex.Message}");
                }

                try
                {
                    // Try to validate the token.

                    var tokenValidationParameters = new TokenValidationParameters
                    {
                        RequireSignedTokens = true,
                        ValidAudience = await this.AudienceAsync(),
                        ValidateAudience = true,
                        ValidIssuer = await this.IssuerAsync(),
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        IssuerSigningKeys = isserSigningKeys
                    };

                    // Throws if the the token cannot be validated.
                    _jwtSecurityTokenHandlerWrapper.ValidateToken(
                        authorizationBearerToken,
                        tokenValidationParameters);

                    isTokenValid = true;
                }
                catch (SecurityTokenSignatureKeyNotFoundException)
                {
                    // A SecurityTokenSignatureKeyNotFoundException is thrown if the signing keys for
                    // validating the JWT could not be found. This could happen if the issuer has
                    // changed the signing keys since the last time they were retrieved by the
                    // ConfigurationManager. To handle this we ask the ConfigurationManger to refresh
                    // which causes it to retrieve the keys again the next time we ask for them.
                    // Then we retry by asking for the signing keys and validating the token again.
                    // We only retry once.

                    await _oidcConfigurationManager.RequestRefreshAsync();

                    validationRetryCount++;
                }
                catch (Exception ex)
                {
                    return new ApiAuthorizationResult(
                        $"Authorization Failed. {ex.GetType()} caught while validating JWT token."
                        + $"Message: {ex.Message}");
                }

            } while (!isTokenValid && validationRetryCount <= 1);

            // Success result.
            return new ApiAuthorizationResult();
        }

        public async Task<HealthCheckResult> HealthCheckAsync()
        {
            if (string.IsNullOrWhiteSpace(await this.AudienceAsync())
                || string.IsNullOrWhiteSpace(await this.AudienceAsync()))
            {
                return new HealthCheckResult(
                    $"Some or all OpenID connection settings are missing.");
            }

            try
            {
                // Get the singing keys fresh. Not from the cache.
                await _oidcConfigurationManager.RequestRefreshAsync();

                await _oidcConfigurationManager.GetIssuerSigningKeysAsync();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(
                    "Problem getting signing keys from Open ID Connect provider (issuer)."
                    + $" ConfigurationManager threw {ex.GetType()} Message: {ex.Message}");
            }

            return new HealthCheckResult(); // Good health.
        }
    }
}
