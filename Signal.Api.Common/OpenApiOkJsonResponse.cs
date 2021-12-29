using System;
using System.Net;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace Signal.Api.Common;

public class OpenApiOkJsonResponse : OpenApiResponseWithBodyAttribute
{
    public OpenApiOkJsonResponse(Type bodyType) : base(HttpStatusCode.OK, "application/json", bodyType)
    {
    }
}