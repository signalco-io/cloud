using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Signal.Infrastructure.ApiAuth.Oidc.Models;

namespace Signal.Infrastructure.ApiAuth.Oidc.Abstractions
{
    public interface IApiAuthorization
    {
        Task<ApiAuthorizationResult> AuthorizeAsync(IHeaderDictionary httpRequestHeaders);

        Task<HealthCheckResult> HealthCheckAsync();
    }
}
