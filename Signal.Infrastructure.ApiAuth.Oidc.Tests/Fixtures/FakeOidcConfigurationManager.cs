using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Signal.Infrastructure.ApiAuth.Oidc.Abstractions;

namespace Signal.Infrastructure.ApiAuth.Oidc.Tests.TestFixtures
{
    public class FakeOidcConfigurationManager : IOidcConfigurationManager
    {
        public string ExceptionMessageForTest { get; set; }

        public int GetIssuerSigningKeysAsyncCalledCount { get; set; }

        public int RequestRefreshCalledCount { get; set; }

        public IEnumerable<SecurityKey> SecurityKeysForTest { get; set; }

        public async Task<IEnumerable<SecurityKey>> GetIssuerSigningKeysAsync()
        {
            ++GetIssuerSigningKeysAsyncCalledCount;

            if (ExceptionMessageForTest != null)
            {
                throw new TestException(ExceptionMessageForTest);
            }
            return await Task.FromResult(SecurityKeysForTest);
        }

        public void RequestRefresh()
        {
            ++RequestRefreshCalledCount;
        }
    }
}
