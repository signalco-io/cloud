using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Signal.Api.Common;
using Signal.Api.Common.Auth;
using Signal.Api.Common.Exceptions;
using Signal.Core.Exceptions;

namespace Signal.Api.Public.Functions.Beacons;

public class StationRefreshTokenFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;

    public StationRefreshTokenFunction(
        IFunctionAuthenticator functionAuthenticator)
    {
        this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
    }

    [FunctionName("Station-RefreshToken")]
    [OpenApiOperation(nameof(StationRefreshTokenFunction), "Station", Description = "Refreshes the access token.")]
    [OpenApiJsonRequestBody<BeaconRefreshTokenRequestDto>]
    [OpenApiOkJsonResponse<BeaconRefreshTokenResponseDto>]
    [OpenApiResponseBadRequestValidation]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "station/refresh-token")]
        HttpRequest req,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = await req.ReadAsJsonAsync<BeaconRefreshTokenRequestDto>();
            if (request == null || 
                string.IsNullOrWhiteSpace(request.RefreshToken))
                throw new ExpectedHttpException(HttpStatusCode.BadRequest, "\"RefreshToken\" property is required.");

            var token = await this.functionAuthenticator.RefreshTokenAsync(req, request.RefreshToken, cancellationToken);
            return new OkObjectResult(new BeaconRefreshTokenResponseDto(token.AccessToken, token.Expire));
        }
        catch (ExpectedHttpException ex)
        {
            return new ObjectResult(new ApiErrorDto(ex.Code.ToString(), ex.Message))
            {
                StatusCode = (int)ex.Code
            };
        }
    }

    [Serializable]
    private class BeaconRefreshTokenRequestDto
    {
        [Required]
        [JsonPropertyName("refreshToken")]
        public string? RefreshToken { get; set; }
    }

    [Serializable]
    private class BeaconRefreshTokenResponseDto
    {
        public BeaconRefreshTokenResponseDto(string accessToken, DateTime expire)
        {
            this.AccessToken = accessToken;
            this.Expire = expire;
        }

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; }

        [JsonPropertyName("expire")]
        public DateTime Expire { get; }
    }
}