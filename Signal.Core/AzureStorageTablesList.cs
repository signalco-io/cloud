using System.Collections.Generic;

namespace Signal.Core
{
    public class AzureStorageTablesList : ItemsList<string>
    {
        public AzureStorageTablesList(IEnumerable<string> items) : base(items)
        {
        }
    }
}
