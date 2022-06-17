using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace Signal.Api.Common;

public class OpenApiJsonRequestBodyAttribute<T> : OpenApiRequestBodyAttribute
{
    public OpenApiJsonRequestBodyAttribute() : base("application/json", typeof(T))
    {
    }
}