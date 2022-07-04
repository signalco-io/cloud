using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Signal.Api.Common.Auth;
using Signal.Api.Common.Contact;
using Signal.Api.Common.Exceptions;
using Signal.Api.Common.OpenApi;
using Signal.Core.Contacts;
using Signal.Core.Entities;
using Signal.Core.Exceptions;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Contacts;

public class ContactSetFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IEntityService entityService;
    private readonly IAzureStorage storage;

    public ContactSetFunction(
        IFunctionAuthenticator functionAuthenticator,
        IEntityService entityService,
        IAzureStorage storage)
    {
        this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    [FunctionName("Contacts-Set")]
    [OpenApiSecurityAuth0Token]
    [OpenApiOperation<ContactSetFunction>("Contact", Description = "Sets contact value.")]
    [OpenApiJsonRequestBody<ContactSetDto>]
    [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "contacts/set")]
        HttpRequest req,
        [SignalR(HubName = "contacts")] 
        IAsyncCollector<SignalRMessage> signalRMessages,
        ILogger logger,
        CancellationToken cancellationToken) =>
        await req.UserRequest<ContactSetDto>(cancellationToken, this.functionAuthenticator, async context =>
        {
            var payload = context.Payload;
            if (string.IsNullOrWhiteSpace(payload.ChannelName) ||
                string.IsNullOrWhiteSpace(payload.ContactName) ||
                string.IsNullOrWhiteSpace(payload.EntityId))
                throw new ExpectedHttpException(
                    HttpStatusCode.BadRequest,
                    "EntityId, ChannelName and ContactName properties are required.");

            await context.ValidateUserAssignedAsync(this.entityService, payload.EntityId);

            // Persist as current state
            // TODO: Use service
            // TODO: Persist history and contact at the same time, wait only for contact to continue to notify and then wait all
            var updateCurrentStateTask = this.storage.UpsertAsync(
                new Contact(
                    payload.EntityId,
                    payload.ChannelName,
                    payload.ContactName,
                    payload.ValueSerialized,
                    payload.TimeStamp ?? DateTime.UtcNow),
                cancellationToken);

            // Wait for current state update before triggering notification
            await updateCurrentStateTask;

            Task? persistHistoryTask = null;
            Task? notifyStateChangeTask = null;
            try
            {
                // Persist to history 
                // TODO: Use service
                // TODO: persist only if given contact is marked for history tracking
                persistHistoryTask = this.storage.UpsertAsync(
                    new ContactHistoryItem(
                        new ContactPointer(
                        payload.EntityId,
                        payload.ChannelName,
                        payload.ContactName),
                        payload.ValueSerialized,
                        payload.TimeStamp ?? DateTime.UtcNow),
                    cancellationToken);

                // Notify listeners
                notifyStateChangeTask = signalRMessages.AddAsync(new SignalRMessage
                {
                    UserId = context.User.UserId,
                    Arguments = new object[]
                    {
                        payload
                    },
                    Target = "contact"
                }, cancellationToken);

                // Wait for all to finish
                await Task.WhenAll(
                    notifyStateChangeTask,
                    persistHistoryTask);
            }
            catch (Exception ex)
            {
                if (notifyStateChangeTask?.IsFaulted ?? false)
                    logger.LogError(ex, "Failed to notify state change.");
                else if (persistHistoryTask?.IsFaulted ?? false)
                    logger.LogError(ex, "Failed to persist state to history.");
                else logger.LogError(ex, "Failed to notify or persist state to history.");
            }
        });
}