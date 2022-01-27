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
        private readonly IConfiguration configuration;

        // TODO: Move to configuration
        private const string KeyVaultUrl = "https://signal.vault.azure.net/";

        private static readonly SecretClient Client;
        private static readonly Dictionary<string, string> Cache = new();

        static SecretsProvider()
        {
            Client = new SecretClient(
                new Uri(KeyVaultUrl),
                new DefaultAzureCredential());
        }

        public SecretsProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        
        public async Task<string> GetSecretAsync(string key, CancellationToken cancellationToken)
        {
            if (Cache.ContainsKey(key))
                return Cache[key];

            try
            {
                var secretFromConfig = this.configuration[key] ?? throw new Exception("Not a local secret.");
                Cache[key] = secretFromConfig;
                return secretFromConfig;
            }
            catch
            {
                // Shit, try next
            }

            try
            {
                var alternativeKey = key.Replace("--", ":");
                var secretFromConfig = this.configuration[alternativeKey] ?? throw new Exception("Not a local secret (alternative name).");
                Cache[key] = secretFromConfig;
                return secretFromConfig;
            }
            catch
            {
                // Shit, try in vault
            }

            var secret = await Client.GetSecretAsync(key, cancellationToken: cancellationToken);
            return Cache[key] = secret.Value.Value;
        }
    }
}
