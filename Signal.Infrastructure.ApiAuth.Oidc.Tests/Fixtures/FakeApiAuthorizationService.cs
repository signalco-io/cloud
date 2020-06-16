using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Signal.Infrastructure.ApiAuth.Oidc.Abstractions;
using Signal.Infrastructure.ApiAuth.Oidc.Models;

namespace Signal.Infrastructure.ApiAuth.Oidc.Tests.TestFixtures
{
    public class FakeApiAuthorizationService : IApiAuthorization
    {
        public ApiAuthorizationResult ApiAuthorizationResultForTests { get; set; }

        public string BadHealthMessageForTests { get; set; }


        public async Task<ApiAuthorizationResult> AuthorizeAsync(IHeaderDictionary httpRequestHeaders)
        {
            return await Task.FromResult(ApiAuthorizationResultForTests);
        }

        public async Task<HealthCheckResult> HealthCheckAsync()
        {
            return await Task.FromResult(
                new HealthCheckResult(BadHealthMessageForTests));
        }
    }
}
