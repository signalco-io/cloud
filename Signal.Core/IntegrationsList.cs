using System.Collections.Generic;

namespace Signal.Core
{

    public class IntegrationsList : ItemsList<string>, IIntegrationsList
    {
        public IntegrationsList(IEnumerable<string> items) : base(items)
        {
        }
    }
}
