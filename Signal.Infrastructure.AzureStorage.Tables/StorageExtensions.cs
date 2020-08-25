using Microsoft.Extensions.DependencyInjection;
using Signal.Core;

namespace Signal.Infrastructure.AzureStorage.Tables
{
    public static class StorageExtensions
    {
        public static void AddStorage(this IServiceCollection services)
        {
            services.AddTransient<IAzureStorage, AzureStorage>();
        }
    }
}