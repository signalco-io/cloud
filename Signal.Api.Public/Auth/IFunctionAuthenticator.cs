using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Signal.Core;

namespace Signal.Api.Public
{
    public interface IFunctionAuthenticator
    {
        Task<IUserAuth> AuthenticateAsync(HttpRequest request, CancellationToken cancellationToken);
    }
}