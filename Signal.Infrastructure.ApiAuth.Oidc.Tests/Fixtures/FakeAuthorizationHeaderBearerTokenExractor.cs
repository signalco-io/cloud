using Microsoft.AspNetCore.Http;
using Signal.Infrastructure.ApiAuth.Oidc.Abstractions;

namespace Signal.Infrastructure.ApiAuth.Oidc.Tests.TestFixtures
{
    public class FakeAuthorizationHeaderBearerTokenExractor : IAuthorizationHeaderBearerTokenExtractor
    {
        public string TokenToReturn { get; set; }

        public string GetToken(IHeaderDictionary httpRequestHeaders)
        {
            return TokenToReturn;
        }
    }
}
