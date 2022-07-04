using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Signal.Core.Secrets;

namespace Signal.Infrastructure.Secrets;

public class SecretsProvider : ISecretsProvider
{
    private const string KeyVaultUrlKey = "SignalcoKeyVaultUrl";

    private readonly Lazy<IConfiguration> configuration;

    private static SecretClient? client;
    private static readonly Dictionary<string, (DateTime expiry, string data)> Cache = new();
    private static readonly TimeSpan VaultCachePersistDurationMs = TimeSpan.FromHours(1);

    public SecretsProvider(Lazy<IConfiguration> configuration)
    {
        this.configuration = configuration;
    }

    protected SecretClient Client()
    {
        return client ??= new SecretClient(
            new Uri(this.configuration.Value[KeyVaultUrlKey]),
            new DefaultAzureCredential());
    }

    public async Task<string> GetSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        // Check cache
        if (Cache.ContainsKey(key))
        {
            var item = Cache[key];
            if (item.expiry > DateTime.UtcNow)
            {
                return Cache[key].data;
            }
        }

        // Check configuration (never expires - required redeployment)
        try
        {
            return this.configuration.Value[key] ?? throw new Exception("Not a local secret.");
        }
        catch
        {
            // Try in vault next
        }

        // Instantiate secrets client if not already
        var secret = await this.Client().GetSecretAsync(key, cancellationToken: cancellationToken);
        return (Cache[key] = (DateTime.UtcNow.Add(VaultCachePersistDurationMs), secret.Value.Value)).data;
    }
}