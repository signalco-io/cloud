using System.Net.Http;

namespace Signal.Api.Public
{
    public static class ExpectedHttpExceptionExtensions
    {
        public static HttpResponseMessage CreateErrorResponseMessage(this ExpectedHttpException ex, HttpRequestMessage request)
        {
            var result = request.CreateErrorResponse(ex.Code, ex.Message);
            ex.ApplyResponseDetails(result);
            return result;
        }
    }
}