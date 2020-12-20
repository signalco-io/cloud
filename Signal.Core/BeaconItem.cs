using System;

namespace Signal.Core
{
    public class BeaconItem : ITableEntity
    {
        public DateTime RegisteredTimeStamp { get; set; }
        
        public string PartitionKey { get; set; }
        
        public string RowKey { get; set; }
    }
}