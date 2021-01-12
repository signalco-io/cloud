using System;

namespace Signal.Core
{
    public class DeviceStateHistoryTableEntity : IDeviceStateHistoryTableEntity
    {
        public DeviceStateHistoryTableEntity(
            string deviceId,
            string channelName,
            string contactName,
            string? valueSerialized,
            DateTime timeStamp)
        {
            this.PartitionKey = $"{deviceId}-{channelName}-{contactName}";
            this.RowKey = timeStamp.ToString("O");
            this.ValueSerialized = valueSerialized;
        }
        
        public string PartitionKey { get; }
        
        public string RowKey { get; }
        
        public string? ValueSerialized { get; }
    }
}