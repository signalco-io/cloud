using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Signal.Infrastructure.ApiAuth.Oidc.Abstractions
{
    public interface IJwtSecurityTokenHandlerWrapper
    {
        void ValidateToken(string token, TokenValidationParameters tokenValidationParameters);
    }
}
