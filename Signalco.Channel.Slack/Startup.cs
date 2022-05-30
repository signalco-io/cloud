using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Signal.Api.Common.Auth;
using Signal.Core;
using Signal.Infrastructure.Secrets;
using Signalco.Channel.Slack;
using Signalco.Channel.Slack.Functions;
using Signalco.Channel.Slack.Functions.Events;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Signalco.Channel.Slack;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services
            .AddTransient<ISecretsProvider, SecretsProvider>()
            .AddTransient<ISlackRequestHandler, SlackRequestHandler>()
            .AddSingleton<IFunctionAuthenticator, FunctionAuth0Authenticator>()
            .AddCore();
    }

    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
    }
}