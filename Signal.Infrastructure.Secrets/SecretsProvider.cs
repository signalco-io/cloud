using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Signal.Core;

namespace Signal.Infrastructure.Secrets
{
    public class SecretsProvider : ISecretsProvider
    {
        private const string KeyVaultUrl = "https://signal.vault.azure.net/";

        public async Task<string> GetSecretAsync(string key, CancellationToken cancellationToken)
        {
            var client = new SecretClient(
                new Uri(KeyVaultUrl),
                new DefaultAzureCredential());
            var secret = await client.GetSecretAsync(key, cancellationToken: cancellationToken);
            return secret.Value.Value;
        }
    }
}
