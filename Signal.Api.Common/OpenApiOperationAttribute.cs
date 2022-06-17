using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace Signal.Api.Common;

public class OpenApiOperationAttribute<T> : OpenApiOperationAttribute
{
    public OpenApiOperationAttribute(params string[] tags) : base(nameof(T), tags)
    {
    }
}