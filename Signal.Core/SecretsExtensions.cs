using Microsoft.Extensions.DependencyInjection;
using Signal.Core.Sharing;

namespace Signal.Core
{
    public static class CoreExtensions
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            return services.AddTransient<ISharingService, SharingService>();
        }
    }
}
