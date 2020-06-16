using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Signal.Infrastructure.ApiAuth.Oidc;

[assembly: FunctionsStartup(typeof(Signal.Api.Startup))]
namespace Signal.Api
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddApiAuthOidc();
        }
    }
}
