using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Signal.Api.Common.Auth;
using Signal.Core;
using Signal.Infrastructure.Secrets;
using Signalco.Channel.Slack;
using Signalco.Channel.Slack.Functions;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Signalco.Channel.Slack;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services
            .AddSecrets()
            .AddCore()
            .AddTransient<ISlackRequestHandler, SlackRequestHandler>()
            .AddSingleton<IFunctionAuthenticator, FunctionAuth0Authenticator>();
    }

    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
    }
}