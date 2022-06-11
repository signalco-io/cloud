using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Signal.Core.Contacts;
using Signal.Core.Users;

namespace Signal.Core.Entities;

public interface IEntityService
{
    Task<string> UpsertAsync(string userId, string? id, Func<string, IEntity> entityFunc, CancellationToken cancellationToken);

    public Task<IDictionary<string, IContact>> ContactsAsync(
        IEnumerable<string> entityIds,
        CancellationToken cancellationToken);

    Task RemoveAsync(string userId, string id, CancellationToken cancellationToken);

    Task<bool> IsUserAssignedAsync(string userId, string id, CancellationToken cancellationToken);

    Task<Dictionary<string, IEnumerable<IUser>>> EntityUsersAsync(IEnumerable<string> entityIds, CancellationToken cancellationToken);
}