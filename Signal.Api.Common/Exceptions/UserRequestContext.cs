using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Signal.Core.Auth;
using Signal.Core.Entities;
using Signal.Core.Exceptions;

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

    public async Task ValidateUserAssignedAsync(IEntityService entityService, string id)
    {
        if (!await entityService.IsUserAssignedAsync(this.User.UserId, id, this.CancellationToken))
            throw new ExpectedHttpException(HttpStatusCode.NotFound);
    }
}