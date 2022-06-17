using System;
using System.Net;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace Signal.Api.Common;

public class OpenApiOkJsonResponseAttribute : OpenApiResponseWithBodyAttribute
{
    public OpenApiOkJsonResponseAttribute(Type bodyType) : base(HttpStatusCode.OK, "application/json", bodyType)
    {
    }
}

public class OpenApiOkJsonResponseAttribute<T> : OpenApiResponseWithBodyAttribute
{
    public OpenApiOkJsonResponseAttribute() : base(HttpStatusCode.OK, "application/json", typeof(T))
    {
    }
}