using Signal.Core;
using Signal.Core.Processes;

namespace Signal.Infrastructure.AzureStorage.Tables
{
    internal class AzureProcessTableEntity : AzureTableEntityBase, IProcessTableEntity
    {
        public string Alias { get; set; }
        
        public bool IsDisabled { get; set; }
        
        public string? ConfigurationSerialized { get; set; }
    }
}