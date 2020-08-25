using System.Collections.Generic;

namespace Signal.Core
{
    public class AzureStorageQueuesList : ItemsList<string>
    {
        public AzureStorageQueuesList(IEnumerable<string> items) : base(items)
        {
        }
    }
}
