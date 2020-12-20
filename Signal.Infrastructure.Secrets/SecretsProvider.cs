using System;
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

        public SecretsProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        
        public async Task<string> GetSecretAsync(string key, CancellationToken cancellationToken)
        {
            try
            {
                return this.configuration[key];
            }
            catch
            {
                // Shit, try in vault
            }

            var client = new SecretClient(
                new Uri(KeyVaultUrl),
                new DefaultAzureCredential());
            var secret = await client.GetSecretAsync(key, cancellationToken: cancellationToken);
            return secret.Value.Value;
        }
    }
}
