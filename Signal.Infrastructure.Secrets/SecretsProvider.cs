using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Signal.Core;

namespace Signal.Infrastructure.Secrets
{
    public class SecretsProvider : ISecretsProvider
    {
        private const string KeyVaultUrlKey = "SignalcoKeyVaultUrl";

        private readonly IConfiguration configuration;

        private static SecretClient? client;
        private static readonly Dictionary<string, string> Cache = new();
        
        public SecretsProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        
        public async Task<string> GetSecretAsync(string key, CancellationToken cancellationToken)
        {
            // Check cache
            if (Cache.ContainsKey(key))
                return Cache[key];

            // Check configuration
            try
            {
                return this.configuration[key] ?? throw new Exception("Not a local secret.");
            }
            catch
            {
                // Shit, try in vault
            }

            // Instantiate secrets client if not already
            client ??= new SecretClient(
                new Uri(this.configuration[KeyVaultUrlKey]),
                new DefaultAzureCredential());
            var secret = await client.GetSecretAsync(key, cancellationToken: cancellationToken);
            return Cache[key] = secret.Value.Value;
        }
    }
}
