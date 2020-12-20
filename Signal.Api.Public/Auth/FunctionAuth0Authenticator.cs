using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Signal.Core;

namespace Signal.Api.Public
{
    public class FunctionAuth0Authenticator : IFunctionAuthenticator
    {
        private readonly ISecretsProvider secretsProvider;
        private readonly ILogger<FunctionAuth0Authenticator> logger;
        private Auth0Authenticator? authenticator;
        
        public FunctionAuth0Authenticator(
            ISecretsProvider secretsProvider,
            ILogger<FunctionAuth0Authenticator> logger)
        {
            this.secretsProvider = secretsProvider ?? throw new ArgumentNullException(nameof(secretsProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task InitializeAuthenticatorAsync(CancellationToken cancellationToken)
        {
            if (this.authenticator != null)
                return;

            var domain = await this.secretsProvider.GetSecretAsync(SecretKeys.Auth0.Domain, cancellationToken);
            var audience = await this.secretsProvider.GetSecretAsync(SecretKeys.Auth0.AppIdentifier, cancellationToken);
            this.logger.LogInformation("Authenticator using domain {Domain}, audience {Audience}", domain, audience);
            this.authenticator = new Auth0Authenticator(domain, new[] {audience});
        }
        
        public async Task<IUserAuth> AuthenticateAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await this.InitializeAuthenticatorAsync(cancellationToken);
                if (this.authenticator == null)
                    throw new NullReferenceException("Authenticator failed to initialize.");
                
                var (user, _) = await this.authenticator.AuthenticateAsync(request, cancellationToken);
                var nameIdentifier = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(nameIdentifier))
                    throw new AuthenticationExpectedHttpException("NameIdentifier claim not present.");

                return new UserAuth(nameIdentifier);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Authorization failed");
                throw new AuthenticationExpectedHttpException(ex.Message);
            }
        }
    }
}