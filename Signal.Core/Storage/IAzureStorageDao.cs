using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Signal.Core.Contacts;
using Signal.Core.Entities;
using Signal.Core.Storage.Blobs;
using Signal.Core.Users;

namespace Signal.Core.Storage;

public interface IAzureStorageDao
{
    Task<IEnumerable<IEntity>> UserEntitiesAsync(string userId, CancellationToken cancellationToken = default);

    Task<IEntity?> GetAsync(string entityId, CancellationToken cancellationToken);

    public Task<IEnumerable<IContact>> ContactsAsync(
        string entityId,
        CancellationToken cancellationToken);

    public Task<IEnumerable<IContact>> ContactsAsync(
        IEnumerable<string> entityIds,
        CancellationToken cancellationToken);

    Task<bool> IsUserAssignedAsync(string userId, string entityId, CancellationToken cancellationToken);

    Task<IEnumerable<IContactHistoryItem>> ContactHistoryAsync(
        IContactPointer contactPointer,
        TimeSpan duration,
        CancellationToken cancellationToken);
    
    Task<IUser?> UserAsync(string userId, CancellationToken cancellationToken = default);

    Task<string?> UserIdByEmailAsync(string userEmail, CancellationToken cancellationToken);

    public Task<IReadOnlyDictionary<string, IEnumerable<string>>> AssignedUsersAsync(
        IEnumerable<string> entityIds,
        CancellationToken cancellationToken);
    
    IAsyncEnumerable<IBlobInfo> LoggingListAsync(string stationId, CancellationToken cancellationToken);

    Task<Stream> LoggingDownloadAsync(string blobName, CancellationToken cancellationToken);

    Task<bool> EntityExistsAsync(string id, CancellationToken cancellationToken);
}