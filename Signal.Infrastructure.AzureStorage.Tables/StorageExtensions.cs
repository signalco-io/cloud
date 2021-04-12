using Microsoft.Extensions.DependencyInjection;
using Signal.Core;
using Signal.Core.Storage;

namespace Signal.Infrastructure.AzureStorage.Tables
{
    public static class StorageExtensions
    {
        public static void AddAzureStorage(this IServiceCollection services)
        {
            services
                .AddTransient<IAzureStorage, AzureStorage>()
                .AddTransient<IAzureStorageDao, AzureStorageDao>();
        }
    }
}