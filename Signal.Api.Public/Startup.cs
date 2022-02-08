using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Signal.Api.Common.HCaptcha;
using Signal.Api.Public;
using Signal.Api.Public.Auth;
using Signal.Core;
using Signal.Infrastructure.AzureStorage.Tables;
using Signal.Infrastructure.Secrets;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Signal.Api.Public;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services
            .AddTransient<ISecretsProvider, SecretsProvider>()
            .AddSingleton<IFunctionAuthenticator, FunctionAuth0Authenticator>()
            .AddAzureStorage()
            .AddCore()
            .AddHCaptcha();
    }

    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
    }
}