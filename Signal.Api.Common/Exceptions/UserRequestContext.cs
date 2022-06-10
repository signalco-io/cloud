using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Signal.Core;
using Signal.Core.Auth;
using Signal.Core.Exceptions;
using Signal.Core.Storage;

namespace Signal.Api.Common.Exceptions;

public class UserRequestContext
{
    public UserRequestContext(IUserAuth user, CancellationToken cancellationToken)
    {
        User = user ?? throw new ArgumentNullException(nameof(user));
        CancellationToken = cancellationToken;
    }

    public IUserAuth User { get; }

    public CancellationToken CancellationToken { get; }

    public async Task ValidateUserAssignedAsync(IEntityService entityService, TableEntityType entityType, string id)
    {
        if (!await entityService.IsUserAssignedAsync(this.User.UserId, entityType, id, this.CancellationToken))
            throw new ExpectedHttpException(HttpStatusCode.NotFound);
    }
}