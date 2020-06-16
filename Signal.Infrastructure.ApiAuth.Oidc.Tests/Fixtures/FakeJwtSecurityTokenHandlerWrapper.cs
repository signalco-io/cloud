using System;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Signal.Infrastructure.ApiAuth.Oidc.Abstractions;

namespace Signal.Infrastructure.ApiAuth.Oidc.Tests.TestFixtures
{
    public class FakeJwtSecurityTokenHandlerWrapper : IJwtSecurityTokenHandlerWrapper
    {
        /// <summary>
        /// Indicates whether or not a SecurityTokenSignatureKeyNotFoundException
        /// should be thrown the first time that ValidateToken(..) is called.
        /// </summary>
        public bool ThrowFirstTime { get; set; }

        public Exception ExceptionToThrow { get; set; }

        public int ValidateTokenCalledCount { get; private set; }

        // IJwtSecurityTokenHandlerWrapper members

        public void ValidateToken(
            string token,
            TokenValidationParameters tokenValidationParameters)
        {
            ++ValidateTokenCalledCount;
            if (ValidateTokenCalledCount == 1
                && ThrowFirstTime)
            {
                throw new SecurityTokenSignatureKeyNotFoundException();
            }

            if (ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }
        }
    }
}
