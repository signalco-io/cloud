using System;

namespace Signal.Core.Devices
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
            this.RowKey = $"{DateTime.MaxValue.Ticks - timeStamp.Ticks:D19}"; // Storing in inverted ticks (see Azure Tables Storage Log tail pattern)
            this.ValueSerialized = valueSerialized;
        }
        
        public string PartitionKey { get; }
        
        public string RowKey { get; }
        
        public string? ValueSerialized { get; }

        public DateTimeOffset? Timestamp { get; }
    }
}