using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Signal.Core;

namespace Signal.Api.Public
{
    public class Auth0Service
    {
        private readonly HttpClient httpClient;
        private readonly ISecretsProvider secretsProvider;


        public Auth0Service(
            HttpClient httpClient,
            ISecretsProvider secretsProvider)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.secretsProvider = secretsProvider ?? throw new ArgumentNullException(nameof(secretsProvider));
        }


        public async Task<Auth0UserInfoDto> Auth0UserInfo(string authHeader, CancellationToken cancellationToken)
        {
            var domain = this.secretsProvider.GetSecretAsync(SecretKeys.Auth0.Domain, cancellationToken);
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://{domain}/userinfo");
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(authHeader);
            using var result = await this.httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            return await result.Content.ReadAsAsync<Auth0UserInfoDto>(cancellationToken);
        }
    }
}
