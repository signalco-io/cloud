using System;
using Signal.Api.Handlers;
using Signal.Core;

namespace Signal.Api.System.Integrations.List
{
    public class IntegrationsListHandler : ServiceHandler<IntegrationsListRequest, IntegrationsListResponse, IIntegrationsService, IIntegrationsList>
    {
        public IntegrationsListHandler(IServiceProvider serviceProvider) : base(serviceProvider, (req, service, cancellation) => service.ListAsync(cancellation))
        {
        }
    }
}
