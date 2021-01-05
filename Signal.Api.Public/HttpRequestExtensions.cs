using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Signal.Api.Public.Auth;
using Signal.Core.Auth;

namespace Signal.Api.Public
{
    public static class HttpRequestExtensions
    {
        public static async Task<T> ReadAsJsonAsync<T>(this HttpRequest req)
        {
            var requestContent = await req.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(requestContent))
                throw new ExpectedHttpException(HttpStatusCode.BadRequest, "Request empty.");

            return JsonSerializer.Deserialize<T>(
                requestContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public static Task<IActionResult> UserRequest<TPayload>(
            this HttpRequest req,
            IFunctionAuthenticator authenticator,
            Func<IUserAuth, TPayload, Task> executionBody,
            CancellationToken cancellationToken) =>
            UserRequest<TPayload>(req, authenticator, async (user, payload) =>
            {
                await executionBody(user, payload);
                return new OkResult();
            }, cancellationToken);

        public static Task<IActionResult> UserRequest<TPayload, TResponse>(
            this HttpRequest req,
            IFunctionAuthenticator authenticator,
            Func<IUserAuth, TPayload, Task<TResponse>> executionBody,
            CancellationToken cancellationToken) =>
            UserRequest<TPayload>(req, authenticator, async (user, payload) =>
            {
                var response = await executionBody(user, payload);
                return new OkObjectResult(response);
            }, cancellationToken);

        public static async Task<IActionResult> UserRequest<TPayload>(
            this HttpRequest req,
            IFunctionAuthenticator authenticator,
            Func<IUserAuth, TPayload, Task<IActionResult>> executionBody,
            CancellationToken cancellationToken)
        {
            try
            {
                var user = await authenticator.AuthenticateAsync(req, cancellationToken);

                // Deserialize and validate
                var payload = await req.ReadAsJsonAsync<TPayload>();
                if (payload == null)
                    throw new ExpectedHttpException(HttpStatusCode.BadRequest, "Failed to read request data.");

                return await executionBody(user, payload);
            }
            catch (ExpectedHttpException ex)
            {
                return new ObjectResult(new ApiErrorDto(ex.Code.ToString(), ex.Message))
                {
                    StatusCode = (int)ex.Code
                };
            }
        }
    }
}