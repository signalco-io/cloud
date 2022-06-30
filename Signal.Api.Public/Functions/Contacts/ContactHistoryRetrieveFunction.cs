using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
using Signal.Core.Contacts;
using Signal.Core.Entities;
using Signal.Core.Exceptions;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Contacts;

public class ContactHistoryRetrieveFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IEntityService entityService;
    private readonly IAzureStorageDao storage;

    public ContactHistoryRetrieveFunction(
        IFunctionAuthenticator functionAuthenticator,
        IEntityService entityService,
        IAzureStorageDao storage)
    {
        this.functionAuthenticator =
            functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    [FunctionName("Contacts-HistoryRetrieve")]
    [OpenApiSecurityAuth0Token]
    [OpenApiOperation<ContactHistoryRetrieveFunction>("Contact", Description = "Retrieves the contact history for provided duration.")]
    [OpenApiJsonRequestBody<ContactHistoryRequestDto>]
    [OpenApiOkJsonResponse<ContactHistoryResponseDto>]
    [OpenApiResponseBadRequestValidation]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "contacts/history")]
        HttpRequest req,
        CancellationToken cancellationToken) =>
        await req.UserRequest<ContactHistoryRequestDto, ContactHistoryResponseDto>(cancellationToken, this.functionAuthenticator, async context =>
        {
            var entityId = context.Payload.EntityId;
            var channelName = context.Payload.ChannelName;
            var contactName = context.Payload.ContactName;
            var duration = context.Payload.Duration;

            if (string.IsNullOrWhiteSpace(entityId) ||
                string.IsNullOrWhiteSpace(channelName) ||
                string.IsNullOrWhiteSpace(contactName))
                throw new ExpectedHttpException(HttpStatusCode.BadRequest, "Required fields not provided.");

            await context.ValidateUserAssignedAsync(this.entityService, entityId);

            var data = await this.storage.ContactHistoryAsync(
                new ContactPointer(entityId, channelName, contactName),
                TimeSpan.TryParse(duration, out var durationValue) ? durationValue : TimeSpan.FromDays(1),
                cancellationToken);

            return new ContactHistoryResponseDto
            {
                Values = data.Select(d => new ContactHistoryResponseDto.TimeStampValuePair
                {
                    TimeStamp = d.Timestamp,
                    ValueSerialized = d.ValueSerialized
                }).ToList()
            };
        });

    [Serializable]
    private class ContactHistoryRequestDto
    {
        [Required]
        [JsonPropertyName("entityId")]
        public string? EntityId { get; set; }

        [Required]
        [JsonPropertyName("channelName")]
        public string? ChannelName { get; set; }

        [Required]
        [JsonPropertyName("contactName")]
        public string? ContactName { get; set; }

        [JsonPropertyName("duration")]
        public string? Duration { get; set; }
    }

    [Serializable]
    private class ContactHistoryResponseDto
    {
        [JsonPropertyName("values")]
        public List<TimeStampValuePair> Values { get; set; } = new();

        [Serializable]
        public class TimeStampValuePair
        {
            [JsonPropertyName("timeStamp")]
            public DateTime TimeStamp { get; set; }

            [JsonPropertyName("valueSerialized")]
            public string? ValueSerialized { get; set; }
        }
    }
}