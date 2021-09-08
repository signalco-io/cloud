using System;
using Signal.Core.Beacon;

namespace Signal.Core.Devices
{
    public class BeaconTableEntity : IBeaconTableEntity
    {
        public string PartitionKey { get; }

        public string RowKey { get; }

        public DateTime RegisteredTimeStamp { get; set; }
        
        public string? Version { get; set; }

        public DateTime? StateTimeStamp { get; set; }

        public BeaconTableEntity(string userId, string id)
        {
            this.PartitionKey = userId;
            this.RowKey = id;
        }
    }
}