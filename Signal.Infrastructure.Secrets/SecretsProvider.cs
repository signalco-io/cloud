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

    // TODO: Use in-memory cache instead of static
    // TODO: Expire cached items
    private static SecretClient? client;
    private static readonly Dictionary<string, string> Cache = new();
        
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
            return Cache[key];

        // Check configuration
        try
        {
            return this.configuration.Value[key] ?? throw new Exception("Not a local secret.");
        }
        catch
        {
            // Shit, try in vault
        }

        // Instantiate secrets client if not already
        var secret = await this.Client().GetSecretAsync(key, cancellationToken: cancellationToken);
        return Cache[key] = secret.Value.Value;
    }
}