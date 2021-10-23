using System;
using System.Collections.Generic;
using Signal.Core.Beacon;

namespace Signal.Infrastructure.AzureStorage.Tables
{
    internal class AzureBeaconTableEntity : AzureTableEntityBase, IBeaconTableEntity
    {
        public DateTime RegisteredTimeStamp { get; set; }

        public string? Version { get; set; }

        public DateTime? StateTimeStamp { get; set; }

        public IEnumerable<string> AvailableWorkerServices { get; set; }

        public IEnumerable<string> RunningWorkerServices { get; set; }
    }
}