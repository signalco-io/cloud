using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Signal.Core.Storage;

namespace Signal.Core.Sharing;

public class SharingService : ISharingService
{
    private readonly IAzureStorage azureStorage;
    private readonly IAzureStorageDao azureDao;
    private readonly ILogger<SharingService> logger;

    public SharingService(
        IAzureStorage azureStorage,
        IAzureStorageDao azureDao,
        ILogger<SharingService> logger)
    {
        this.azureStorage = azureStorage ?? throw new ArgumentNullException(nameof(azureStorage));
        this.azureDao = azureDao ?? throw new ArgumentNullException(nameof(azureDao));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task AssignToUserEmailAsync(
        string userEmail, 
        string entityId,
        CancellationToken cancellationToken)
    {
        var sanitizedEmail = userEmail.Trim().ToLowerInvariant();
        var userId = await this.azureDao.UserIdByEmailAsync(sanitizedEmail, cancellationToken);
        if (!string.IsNullOrWhiteSpace(userId))
            await this.AssignToUserAsync(userId, entityId, cancellationToken);

        this.logger.LogWarning("Unknown user email {UserEmail}. Didn't assign entity {EntityId}", userEmail, entityId);
    }

    public async Task AssignToUserAsync(string userId, string entityId, CancellationToken cancellationToken)
    {
        // TODO: Check if current user has rights to assign others to provided entity
        // TODO: Check if entity exists

        await this.azureStorage.UpsertAsync(
            new UserAssignedEntity(userId, entityId), cancellationToken);
    }
}