using Microsoft.Extensions.DependencyInjection;
using Signal.Core.Notifications;
using Signal.Core.Sharing;

namespace Signal.Core
{
    public static class CoreExtensions
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            return services
                .AddTransient<INotificationSmtpService, NotificationSmtpService>()
                .AddTransient<INotificationService, NotificationService>()
                .AddTransient<ISharingService, SharingService>()
                .AddTransient<IEntityService, EntityService>();
        }
    }
}
