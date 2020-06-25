using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Signal.Infrastructure.ApiAuth.Oidc;
using Signal.Infrastructure.AzureStorage.Tables;
using Signal.Infrastructure.Secrets;
using Voyager;
using Voyager.Azure.Functions;

[assembly: FunctionsStartup(typeof(Signal.Api.ApiConfig.Startup))]
namespace Signal.Api.ApiConfig
{
    public class Startup : FunctionsStartup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UsePathBase("/api");
            app.UseVoyagerExceptionHandler();
            app.UseRouting();
            app.UseApiAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapVoyager();
            });
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddApiAuthOidc();
            builder.Services.AddSecrets();
            builder.Services.AddStorage();
            builder.AddVoyager(ConfigureServices, Configure);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddVoyager(c =>
            {
                c.AddAssemblyWith<Startup>();
            });
        }
    }
}
