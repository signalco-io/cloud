using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Signal.Api.Common.Auth;

public static class Auth0AuthenticatorExtensions
{
    /// <summary>
    /// Authenticates the user via an "Authentication: Bearer {token}" header.
    /// Returns a user principal containing claims from the token and a token that can be used to perform actions on behalf of the user.
    /// Throws an exception if the token fails to authenticate or if the Authentication header is malformed.
    /// This method has an asynchronous signature, but usually completes synchronously.
    /// </summary>
    /// <param name="this">The authenticator instance.</param>
    /// <param name="header">The authentication header.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    public static async Task<(ClaimsPrincipal User, SecurityToken ValidatedToken)> AuthenticateAsync(
        this Auth0Authenticator @this, AuthenticationHeaderValue header,
        CancellationToken cancellationToken = new())
    {
        if (header == null || !string.Equals(header.Scheme, "Bearer", StringComparison.InvariantCultureIgnoreCase))
            throw new InvalidOperationException("Authentication header does not use Bearer token.");
        if (header.Parameter == null)
            throw new InvalidOperationException("Authentication header parameter is empty.");
        return await @this.AuthenticateAsync(header.Parameter, cancellationToken);
    }

    /// <summary>
    /// Authenticates the user via an "Authentication: Bearer {token}" header in an HTTP request message.
    /// Returns a user principal containing claims from the token and a token that can be used to perform actions on behalf of the user.
    /// Throws an exception if the token fails to authenticate or if the Authentication header is missing or malformed.
    /// This method has an asynchronous signature, but usually completes synchronously.
    /// </summary>
    /// <param name="this">The authenticator instance.</param>
    /// <param name="request">The HTTP request.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    public static Task<(ClaimsPrincipal User, SecurityToken ValidatedToken)> AuthenticateAsync(
        this Auth0Authenticator @this,
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.Headers.ContainsKey("Authorization"))
            throw new InvalidOperationException("Authorization header is required.");

        var authHeader = request.Headers["Authorization"];
        var auth = AuthenticationHeaderValue.Parse(authHeader);
        return @this.AuthenticateAsync(auth, cancellationToken);
    }
}