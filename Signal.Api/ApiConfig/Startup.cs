using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Extensions.Http;
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
            //app.UseApiCors();
            app.UseCors(config =>
            {
                config.AllowAnyHeader();
                config.AllowAnyMethod();
                config.AllowAnyOrigin();
                //config.AllowCredentials();
            });
            app.UsePathBase("/api");
            app.UseVoyagerExceptionHandler();
            app.UseRouting();
            app.UseApiAuthorization();
            //app.UseAuthentication();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapVoyager();
            });
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            //builder.Services.AddDataProtection();
            builder.AddVoyager(ConfigureServices, Configure);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Fix for: https://github.com/Azure/azure-functions-host/issues/5447
            //var dataProtectionHostedService = services.First(x => x.ImplementationType?.Name.Contains("DataProtectionHostedService") ?? false);
            //services.Remove(dataProtectionHostedService);

            services.AddApiAuthOidc();
            services.AddSecrets();
            services.AddStorage();
            services.AddVoyager(c =>
            {
                c.AddAssemblyWith<Startup>();
            });
            //services.AddAuthentication(options =>
            //{
            //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //})
            //    .AddScriptWebHostJwtBearer()
            //  .AddJwtBearer(options =>
            //{
            //    options.Authority = "https://dfnoise.eu.auth0.com";
            //    options.Audience = "https://api.signal.dfnoise.com";
            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        NameClaimType = ClaimTypes.NameIdentifier
            //    };
            //});
        }
    }

    public static class AppExtensions
    {
        private const string AuthLevelClaimType = "http://schemas.microsoft.com/2017/07/functions/claims/authlevel";
    
        public static AuthenticationBuilder AddScriptWebHostJwtBearer(this AuthenticationBuilder builder)
        {
            return builder.AddJwtBearer("WebJobsAuthLevel", options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = c =>
                    {
                        options.TokenValidationParameters = CreateTokenValidationParameters();
                        return Task.CompletedTask;
                    },
    
                    OnTokenValidated = c =>
                    {
                        c.Principal.AddIdentity(new ClaimsIdentity(new Claim[] { new Claim(AuthLevelClaimType, AuthorizationLevel.Admin.ToString()) }));
                        c.Success();
                        return Task.CompletedTask;
                    }
                };
    
                options.TokenValidationParameters = CreateTokenValidationParameters();
            });
    
            TokenValidationParameters CreateTokenValidationParameters()
            {
                var defaultKey = "2d3a0617-f369-492c-ab7a-f21ec1631376";
                var result = new TokenValidationParameters();
    
                if (defaultKey != null)
                {
                    result.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(defaultKey));
                    result.ValidateAudience = true;
                    result.ValidateIssuer = true;
                    result.ValidAudience = string.Format("https://{0}.azurewebsites.net/azurefunctions", "func");
                    result.ValidIssuer = string.Format("https://{0}.scm.azurewebsites.net", "func");
                }
    
                return result;
            }
        }
    }
}
