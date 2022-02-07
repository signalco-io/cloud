using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Signal.Api.Common.HCaptcha;

public static class HCaptchaHttpRequestExtensions
{
    public static async Task VerifyCaptchaAsync(this HttpRequest req, IHCaptchaService service, CancellationToken cancellationToken)
    {
        if (!req.Headers.TryGetValue("HCAPTCHA-RESPONSE", out var responseValues))
            throw new InvalidOperationException("hCaptcha response not provided.");

        var response = responseValues.ToString();
        await service.VerifyAsync(response, cancellationToken);
    }
}
