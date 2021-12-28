using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Signal.Api.Internal;
using Signal.Core;
using Signal.Infrastructure.AzureStorage.Tables;
using Signal.Infrastructure.Secrets;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Signal.Api.Internal;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services
            .AddTransient<ISecretsProvider, SecretsProvider>()
            .AddAzureStorage();
    }

    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
    }
}