using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Signal.Core.Auth;

namespace Signal.Api.Public.Auth;

public interface IFunctionAuthenticator
{
    Task<IUserAuth> AuthenticateAsync(HttpRequest request, CancellationToken cancellationToken);

    Task<IUserRefreshToken> RefreshTokenAsync(
        HttpRequest request,
        string refreshToken,
        CancellationToken cancellationToken);
}