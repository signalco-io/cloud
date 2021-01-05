using System;
using Signal.Core;

namespace Signal.Infrastructure.AzureStorage.Tables
{
    internal class AzureDeviceStateTableEntity : AzureTableEntityBase, IDeviceStateTableEntity
    {
        public string DeviceIdentifier { get; set; }
        
        public string ChannelName { get; set; }
        
        public string ContactName { get; set; }
        
        public string? ValueSerialized { get; set; }

        public DateTime TimeStamp { get; set; }
    }

    internal class AzureDeviceTableEntity : AzureTableEntityBase, IDeviceTableEntity
    {
        public string DeviceIdentifier { get; set; }

        public string Alias { get; set; }
    }

    internal class AzureUserAssignedEntitiesTableEntry : AzureTableEntityBase
    {
        public string Devices { get; set; }
    }
}