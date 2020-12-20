using System;
using System.Net;
using System.Net.Http;

namespace Signal.Api.Public
{
    public class ExpectedHttpException : Exception
    {
        public ExpectedHttpException(HttpStatusCode code, string message = "")
            : base(message)
        {
            this.Code = code;
        }

        public HttpStatusCode Code { get; }

        public HttpResponseMessage CreateErrorResponseMessage(HttpRequestMessage request)
        {
            var result = request.CreateErrorResponse(this.Code, this.Message);
            this.ApplyResponseDetails(result);
            return result;
        }

        protected virtual void ApplyResponseDetails(HttpResponseMessage response)
        {
        }
    }
}