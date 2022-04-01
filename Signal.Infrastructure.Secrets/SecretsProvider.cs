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
        private const string KeyVaultUrl = "https://vault-kv8e656081.vault.azure.net/";

        private static readonly SecretClient Client;
        private static readonly Dictionary<string, string> Cache = new Dictionary<string, string>();

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
            try
            {
                return this.configuration[key] ?? throw new Exception("Not a local secret.");
            }
            catch
            {
                // Shit, try in vault
            }

            if (Cache.ContainsKey(key)) 
                return Cache[key];

            var secret = await Client.GetSecretAsync(key, cancellationToken: cancellationToken);
            return Cache[key] = secret.Value.Value;
        }
    }
}
