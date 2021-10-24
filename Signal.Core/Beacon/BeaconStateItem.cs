using System;
using System.Collections.Generic;
using Signal.Core.Storage;

namespace Signal.Core.Beacon
{
    public class BeaconStateItem : ITableEntity
    {
        public BeaconStateItem(string userId, string beaconId)
        {
            this.PartitionKey = userId;
            this.RowKey = beaconId;
        }

        public string PartitionKey { get; }

        public string RowKey { get; }

        public string? Version { get; set; }

        public string? AvailableWorkerServices { get; set; }

        public string? RunningWorkerServices { get; set; }

        public DateTime StateTimeStamp { get; set; }
    }
}