using System.Collections.Generic;
using Signal.Api.Dtos;

namespace Signal.Api.System.Integrations.List
{
    public class IntegrationsListResponse : ListResponse<string>
    {
        public IntegrationsListResponse(IEnumerable<string> items) : base(items)
        {
        }
    }
}
