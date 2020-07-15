using System.Collections.Generic;
using System.Threading.Tasks;

namespace Signal.Core
{
    public class IntegrationsService : IIntegrationsService
    {
        public async Task<IIntegrationsList> ListAsync()
        {
            return new IntegrationsList(new List<string>() { "devops" });
        }
    }
}
