using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Signal.Core.Storage;

namespace Signal.Core.Notifications;

internal class NotificationService : INotificationService
{
    private readonly INotificationSmtpService smtpService;
    private readonly IAzureStorageDao storage;

    public NotificationService(
        INotificationSmtpService smtpService,
        IAzureStorageDao storage)
    {
        this.smtpService = smtpService ?? throw new ArgumentNullException(nameof(smtpService));
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    public async Task CreateAsync(
        IEnumerable<string> userIds, 
        NotificationContent content, 
        NotificationOptions? options,
        CancellationToken cancellationToken = default)
    {
        foreach (var userId in userIds)
        {
            try
            {
                var user = await this.storage.UserAsync(userId, cancellationToken);

                // Send email if requested with options (opt-in)
                if (options?.SendEmail ?? false)
                {
                    await this.smtpService.SendAsync(
                        user?.Email ??
                        throw new InvalidOperationException($"Email not available for user {userId}"),
                        content.Title,
                        content.Content?.ToString() ?? string.Empty,
                        cancellationToken);
                }
            }
            catch
            {
                throw;
            }
        }
    }
}