using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Signal.Core.Storage;

namespace Signal.Core.Sharing
{
    public class SharingService : ISharingService
    {
        private readonly IAzureStorage azureStorage;
        private readonly IAzureStorageDao azureDao;

        public SharingService(
            IAzureStorage azureStorage,
            IAzureStorageDao azureDao)
        {
            this.azureStorage = azureStorage ?? throw new ArgumentNullException(nameof(azureStorage));
            this.azureDao = azureDao ?? throw new ArgumentNullException(nameof(azureDao));
        }

        public async Task AssignToUserEmail(
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

        public async Task AssignToUser(string userId, TableEntityType entityType, string entityId, CancellationToken cancellationToken)
        {
            await this.azureStorage.CreateOrUpdateItemAsync(
                ItemTableNames.UserAssignedEntity(entityType),
                new UserAssignedEntity(userId, entityId), cancellationToken);
        }
    }
}
