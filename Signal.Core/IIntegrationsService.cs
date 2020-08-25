using System.Threading.Tasks;

namespace Signal.Core
{
    public interface IIntegrationsService
    {
        Task<IIntegrationsList> ListAsync();
    }
}
