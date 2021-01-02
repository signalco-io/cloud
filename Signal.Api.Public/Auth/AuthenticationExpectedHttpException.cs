using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Signal.Api.Public.Auth
{
    public sealed class AuthenticationExpectedHttpException : ExpectedHttpException
    {
        public AuthenticationExpectedHttpException(string message = "")
            : base(HttpStatusCode.Forbidden, message)
        {
        }

        protected override void ApplyResponseDetails(HttpResponseMessage response)
        {
            response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("Bearer", "token_type=\"JWT\""));
        }
    }
}