using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Signal.Api.Common;
using Signal.Api.Common.Auth;
using Signal.Api.Common.Entities;
using Signal.Api.Common.Exceptions;
using Signal.Api.Common.Users;
using Signal.Core.Entities;

namespace Signal.Api.Public.Functions.Entity;

public class EntitiesRetrieveFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IEntityService entityService;

    public EntitiesRetrieveFunction(
        IFunctionAuthenticator functionAuthenticator,
        IEntityService entityService)
    {
        this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
    }

    [FunctionName("Entities-Retrieve")]
    [OpenApiSecurityAuth0Token]
    [OpenApiOperation<EntitiesRetrieveFunction>("Entity", Description = "Retrieves all available entities.")]
    [OpenApiOkJsonResponse<IEnumerable<EntityDetailsDto>>]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "entity")]
        HttpRequest req,
        CancellationToken cancellationToken) =>
        await req.UserRequest(cancellationToken, functionAuthenticator, async context =>
        {
            var entities = (await entityService.AllAsync(context.User.UserId, cancellationToken)).ToList();
            var contacts = await entityService.ContactsAsync(entities.Select(d => d.Id).ToList(), cancellationToken);
            var entityUsers = await entityService.EntityUsersAsync(
                entities.Select(d => d.Id),
                cancellationToken);

            return entities.Select(d =>
            {
                var users = entityUsers[d.Id]
                    .Select(u => new UserDto(u.UserId, u.Email, u.FullName));

                var contactsDtos = contacts
                    .Where(s => s.EntityId == d.Id)
                    .Select(s => new ContactDto
                    (
                        s.ContactName,
                        s.ChannelName,
                        s.ValueSerialized,
                        s.TimeStamp
                    ));

                return new EntityDetailsDto(d.Id, d.Alias)
                {
                    Contacts = contactsDtos,
                    SharedWith = users
                };
            });
        });
}