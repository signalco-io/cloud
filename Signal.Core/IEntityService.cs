using System;
using System.Threading;
using System.Threading.Tasks;
using Signal.Core.Storage;

namespace Signal.Core
{
    public interface IEntityService
    {
        Task<string> UpsertEntityAsync<TEntity>(string userId, string? entityId, TableEntityType entityType, string tableName, Func<string, TEntity> entityFunc, CancellationToken cancellationToken)
            where TEntity : ITableEntity;

        Task RemoveByIdAsync(string tableName, string rowKey, CancellationToken cancellationToken);

        Task<bool> IsUserAssignedAsync(string userId, TableEntityType entityType, string id, CancellationToken cancellationToken);
    }
}