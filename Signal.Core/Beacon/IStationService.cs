using System.Threading;
using System.Threading.Tasks;

namespace Signal.Core.Beacon;

public interface IStationService
{
    Task RegisterAsync(string email, CancellationToken cancellationToken);
}