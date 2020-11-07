using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Signal.Core
{
    public class IntegrationsService : IIntegrationsService
    {
        public Task<IIntegrationsList> ListAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult((IIntegrationsList)new IntegrationsList(new List<string> { "devops" }));
        }
    }
}
