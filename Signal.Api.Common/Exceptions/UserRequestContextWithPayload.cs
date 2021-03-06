using System.Threading;
using Signal.Core.Auth;

namespace Signal.Api.Common.Exceptions;

public class UserRequestContextWithPayload<TPayload> : UserRequestContext
{
    public TPayload Payload { get; }

    public UserRequestContextWithPayload(IUserAuth user, TPayload payload, CancellationToken cancellationToken) : base(user, cancellationToken)
    {
        this.Payload = payload;
    }
}