using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Signal.Core.Contacts;
using Signal.Core.Entities;
using Signal.Core.Newsletter;
using Signal.Core.Sharing;
using Signal.Core.Users;

namespace Signal.Core.Storage;

public interface IAzureStorage
{
    Task UpsertAsync(IEntity entity, CancellationToken cancellationToken = default);

    Task UpsertAsync(IContactPointer contact, CancellationToken cancellationToken = default);

    Task UpsertAsync(IContact contact, CancellationToken cancellationToken = default);

    Task UpsertAsync(IContactHistoryItem item, CancellationToken cancellationToken = default);

    Task UpsertAsync(IUserAssignedEntity userAssignment, CancellationToken cancellationToken = default);

    Task UpsertAsync(IUser user, CancellationToken cancellationToken = default);

    Task UpsertAsync(INewsletterSubscription subscription, CancellationToken cancellationToken = default);

    Task RemoveEntityAsync(string id, CancellationToken cancellationToken = default);

    Task AppendToFileAsync(string directory, string fileName, Stream data, CancellationToken cancellationToken);
}