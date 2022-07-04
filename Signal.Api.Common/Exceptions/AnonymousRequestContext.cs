using System.Threading;

namespace Signal.Api.Common.Exceptions;

public class AnonymousRequestContext
{
    public AnonymousRequestContext(CancellationToken cancellationToken)
    {
        CancellationToken = cancellationToken;
    }

    public CancellationToken CancellationToken { get; }
}