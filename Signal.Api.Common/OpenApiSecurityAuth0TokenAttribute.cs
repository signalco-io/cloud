using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace Signal.Api.Common;

public sealed class OpenApiSecurityAuth0TokenAttribute : OpenApiSecurityAttribute
{
    public OpenApiSecurityAuth0TokenAttribute() : base(
        "bearer", SecuritySchemeType.Http)
    {
        BearerFormat = "JWT";
        Scheme = OpenApiSecuritySchemeType.Bearer;
        Description = "Auth0 token.";
    }
}