using System;

namespace Signal.Core
{
    public class BeaconItem : ITableEntity
    {
        public string PartitionKey { get; set; }
        
        public string RowKey { get; set; }
        
        public string? Alias { get; set; }
        
        public string? RefreshToken { get; set; }
        
        public DateTime RegisteredTimeStamp { get; set; }
    }
}