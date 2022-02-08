using System;
using System.Threading;
using System.Threading.Tasks;
using Signal.Core;

namespace Signal.Api.Common.HCaptcha;

public class HCaptchaService : IHCaptchaService
{
    private readonly ISecretsProvider secretsProvider;
    private readonly IHCaptchaApi api;


    public HCaptchaService(
        ISecretsProvider secretsProvider,
        IHCaptchaApi api)
    {
        this.secretsProvider = secretsProvider ?? throw new ArgumentNullException(nameof(secretsProvider));
        this.api = api ?? throw new ArgumentNullException(nameof(api));
    }


    public async Task VerifyAsync(string response, CancellationToken cancellationToken)
    {
        var verifyResponse = await this.api.Verify(
            await this.secretsProvider.GetSecretAsync(SecretKeys.HCaptcha.SiteKey, cancellationToken),
            await this.secretsProvider.GetSecretAsync(SecretKeys.HCaptcha.Secret, cancellationToken),
            response,
            cancellationToken);
        if (verifyResponse?.Success ?? false)
            return;

        // TODO: Handle errors with more specific response
        throw new Exception("Invalid hCaptcha response.");
    }
}
