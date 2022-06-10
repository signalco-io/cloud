using System.Threading;
using System.Threading.Tasks;
using Signal.Core.Storage;

namespace Signal.Core.Sharing;

public interface ISharingService
{
    Task AssignToUserAsync(string userId, TableEntityType entityType, string entityId, CancellationToken cancellationToken);

    Task AssignToUserEmailAsync(
        string userEmail, 
        TableEntityType entityType, string entityId,
        CancellationToken cancellationToken);

    Task RemoveAssignmentsAsync(
        TableEntityType entityType, 
        string entityId,
        CancellationToken cancellationToken);
}