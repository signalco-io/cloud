using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Signal.Api.Common.HCaptcha;

public static class HCaptchaHttpRequestExtensions
{
    public const string HCaptchaHeaderKey = "HCAPTCHA-RESPONSE";

    /// <summary>A HttpRequest extension method that verify captcha asynchronous.</summary>
    /// <exception cref="InvalidOperationException">Thrown when the requested operation is invalid.</exception>
    /// <param name="req">The req to act on.</param>
    /// <param name="service">The service.</param>
    /// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
    /// <returns>A Task.</returns>
    public static async Task VerifyCaptchaAsync(this HttpRequest req, IHCaptchaService service, CancellationToken cancellationToken)
    {
        if (!req.Headers.TryGetValue(HCaptchaHeaderKey, out var responseValues))
            throw new InvalidOperationException("hCaptcha response not provided.");

        var response = responseValues.ToString();
        await service.VerifyAsync(response, cancellationToken).ConfigureAwait(false);
    }
}
