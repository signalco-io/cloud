using System;

namespace Signal.Core
{
    public class DeviceStateTableEntity : IDeviceStateTableEntity
    {
        public DeviceStateTableEntity(
            string deviceId,
            string channelName, 
            string contactName, 
            string? valueSerialized, 
            DateTime timeStamp)
        {
            this.PartitionKey = $"{deviceId}";
            this.RowKey = $"{channelName}-{contactName}";
            this.ChannelName = channelName;
            this.ContactName = contactName;
            this.ValueSerialized = valueSerialized;
            this.TimeStamp = timeStamp;
        }

        public string PartitionKey { get; }
        
        public string RowKey { get; }
        
        public string ChannelName { get; }
        
        public string ContactName { get; }
        
        public string? ValueSerialized { get; }
        
        public DateTime TimeStamp { get; }
    }
}