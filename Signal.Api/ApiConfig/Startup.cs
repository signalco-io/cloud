using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
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
            app.UsePathBase("/");
            app.UseVoyagerExceptionHandler();
            app.UseRouting();
            //app.UseApiAuthorization();
            app.UseAuthentication();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapVoyager();
            });
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.AddVoyager(ConfigureServices, Configure);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddApiAuthOidc();
            services.AddSecrets();
            services.AddStorage();
            services.AddVoyager(c =>
            {
                c.AddAssemblyWith<Startup>();
            });
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = "dfnoise.eu.auth0.com";
                options.Audience = "https://api.signal.dfnoise.com";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });
        }
    }
}
