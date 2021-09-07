using System.Threading;
using System.Threading.Tasks;
using Signal.Core.Storage;

namespace Signal.Core.Sharing
{
    public interface ISharingService
    {
        Task AssignToUser(string userId, TableEntityType entityType, string entityId, CancellationToken cancellationToken);

        Task AssignToUserEmail(
            string userEmail, 
            TableEntityType entityType, string entityId,
            CancellationToken cancellationToken);
    }
}