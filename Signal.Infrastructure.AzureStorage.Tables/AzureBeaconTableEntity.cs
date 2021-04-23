using System;
using Signal.Core.Beacon;

namespace Signal.Infrastructure.AzureStorage.Tables
{
    internal class AzureBeaconTableEntity : AzureTableEntityBase, IBeaconTableEntity
    {
        public DateTime RegisteredTimeStamp { get; set; }
    }
}