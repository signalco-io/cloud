using System.Threading;
using System.Threading.Tasks;

namespace Signal.Core;

public interface ISecretsManager : ISecretsProvider
{
    Task SetAsync(string key, string secret, CancellationToken cancellationToken = default);
}