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
using Signal.Api.Common;
using Signal.Api.Common.Auth;
using Signal.Api.Common.Exceptions;
using Signal.Core.Entities;
using Signal.Core.Exceptions;

namespace Signal.Api.Public.Functions.Entity;

public class EntityUpsertFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IEntityService entityService;

    public EntityUpsertFunction(
        IFunctionAuthenticator functionAuthenticator,
        IEntityService entityService)
    {
        this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
    }

    [FunctionName("Entities-Upsert")]
    [OpenApiOperation<EntityUpsertFunction>("Entity", Description = "Creates or updated entity. Will create entity if Id is not provided.")]
    [OpenApiJsonRequestBody<EntityUpsertDto>]
    [OpenApiOkJsonResponse<EntityUpsertResponseDto>]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "entity")]
        HttpRequest req,
        CancellationToken cancellationToken) =>
        await req.UserRequest<EntityUpsertDto, EntityUpsertResponseDto>(cancellationToken, functionAuthenticator, async context =>
        {
            var payload = context.Payload;
            var user = context.User;
            if (string.IsNullOrWhiteSpace(payload.Alias))
                throw new ExpectedHttpException(HttpStatusCode.BadRequest, "Alias property is required.");
            if (payload.Type is null or EntityType.Unknown)
                throw new ExpectedHttpException(HttpStatusCode.BadRequest, "Type property is required and can't be Unknown.");

            var deviceId = await entityService.UpsertAsync(
                user.UserId,
                payload.Id,
                id => new Core.Entities.Entity(
                    payload.Type.Value,
                    id,
                    payload.Alias),
                cancellationToken);

            // TODO: Assign contact for device identifier

            return new EntityUpsertResponseDto(deviceId);
        });

    [Serializable]
    private class EntityUpsertDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [Required]
        [JsonPropertyName("type")]
        public EntityType? Type { get; set; }

        [Required]
        [JsonPropertyName("alias")]
        public string? Alias { get; set; }
    }

    [Serializable]
    private class EntityUpsertResponseDto
    {
        public EntityUpsertResponseDto(string id)
        {
            Id = id;
        }

        [JsonPropertyName("id")]
        public string Id { get; }
    }
}