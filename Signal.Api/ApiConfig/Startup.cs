using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Signal.Core;
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
            app.UseCors(config =>
            {
                config.AllowAnyHeader();
                config.AllowAnyMethod();
                config.AllowAnyOrigin();
            });
            app.UsePathBase("/api");
            app.UseVoyagerExceptionHandler();
            app.UseRouting();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "API V1");
            });
            app.UseApiAuthorization();
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
            services.AddAutoMapper(config =>
            {
                config.AddMaps(new[] {
                    typeof(Startup)
                });
            });
            services.AddApiAuthOidc();
            services.AddCore();
            services.AddSecrets();
            services.AddStorage();
            services.AddVoyager(c =>
            {
                c.AddAssemblyWith<Startup>();
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Api", Version = "v1" });
            });
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
