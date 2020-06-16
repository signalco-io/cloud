using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Signal.Infrastructure.ApiAuth.Oidc;
using Signal.Infrastructure.AzureStorage.Tables;
using Signal.Infrastructure.Secrets;

[assembly: FunctionsStartup(typeof(Signal.Api.Startup))]
namespace Signal.Api
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddApiAuthOidc();
            builder.Services.AddSecrets();
            builder.Services.AddStorage();
        }
    }
}
