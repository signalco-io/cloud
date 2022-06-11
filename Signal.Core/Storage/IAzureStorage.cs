using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Signal.Core.Entities;
using Signal.Core.Sharing;

namespace Signal.Core.Storage;

public interface IAzureStorage
{
    Task UpsertAsync(IEntity entity, CancellationToken cancellationToken = default);

    Task UpsertAsync(IUserAssignedEntity userAssignment, CancellationToken cancellationToken = default);

    Task RemoveEntityAsync(string id, CancellationToken cancellationToken = default);

    Task AppendToFileAsync(string directory, string fileName, Stream data, CancellationToken cancellationToken);
}