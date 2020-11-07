using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
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
            builder.AddVoyager(this.ConfigureServices, this.Configure);
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
}
