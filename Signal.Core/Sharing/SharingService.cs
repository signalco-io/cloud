using System;
using System.Threading;
using System.Threading.Tasks;
using Signal.Core.Storage;

namespace Signal.Core.Sharing;

public class SharingService : ISharingService
{
    private readonly IAzureStorage azureStorage;
    private readonly IAzureStorageDao azureDao;
    private readonly IEntityService entityService;

    public SharingService(
        IAzureStorage azureStorage,
        IAzureStorageDao azureDao,
        IEntityService entityService)
    {
        this.azureStorage = azureStorage ?? throw new ArgumentNullException(nameof(azureStorage));
        this.azureDao = azureDao ?? throw new ArgumentNullException(nameof(azureDao));
        this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
    }

    public async Task AssignToUserEmailAsync(
        string userEmail, 
        TableEntityType entityType, string entityId,
        CancellationToken cancellationToken)
    {
        var sanitizedEmail = userEmail.Trim().ToLowerInvariant();
        var userId = await this.azureDao.UserIdByEmailAsync(sanitizedEmail, cancellationToken);
        if (!string.IsNullOrWhiteSpace(userId))
            await this.azureStorage.CreateOrUpdateItemAsync(
                ItemTableNames.UserAssignedEntity(entityType),
                new UserAssignedEntity(userId, entityId), cancellationToken);
    }

    public async Task AssignToUserAsync(string userId, TableEntityType entityType, string entityId, CancellationToken cancellationToken)
    {
        await this.azureStorage.CreateOrUpdateItemAsync(
            ItemTableNames.UserAssignedEntity(entityType),
            new UserAssignedEntity(userId, entityId), cancellationToken);
    }

    public async Task RemoveAssignmentsAsync(
        TableEntityType entityType,
        string entityId,
        CancellationToken cancellationToken)
    {
        await this.entityService.RemoveByIdAsync(
            ItemTableNames.UserAssignedEntity(entityType), 
            entityId,
            cancellationToken);
    }
}