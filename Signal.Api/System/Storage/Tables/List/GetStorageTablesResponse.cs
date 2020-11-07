using System.Collections.Generic;

namespace Signal.Api.System.Storage.Tables.List
{
    public class GetStorageTablesResponse
    {
        public IEnumerable<string>? Items { get; set; }
    }
}
