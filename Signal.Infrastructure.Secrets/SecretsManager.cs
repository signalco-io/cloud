using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Signal.Core.Secrets;

namespace Signal.Infrastructure.Secrets;

public class SecretsManager : SecretsProvider, ISecretsManager
{
    public SecretsManager(Lazy<IConfiguration> configuration) : base(configuration)
    {
    }

    public async Task SetAsync(string key, string secret, CancellationToken cancellationToken = default)
    {
        try
        {
            var currentSecret = await this.GetSecretAsync(key, cancellationToken: cancellationToken);
            if (currentSecret == secret)
                return;
        }
        catch
        {
            // No secret
        }

        await this.Client().SetSecretAsync(key, secret, cancellationToken);
    }
}