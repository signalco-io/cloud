using System;

namespace Signal.Core
{
    public interface ITableEntityKey
    {
        string PartitionKey { get; set; }

        string RowKey { get; set; }
    }

    public class TableEntityKey : ITableEntityKey
    {
        public TableEntityKey(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
    }
    
    public interface ITableEntity : ITableEntityKey
    {
    }
    
    public interface IDeviceStateHistoryTableEntity : ITableEntity
    {
        string? ValueSerialized { get; set; }
        DateTime TimeStamp { get; set; }
    }
    
    public class DeviceStateHistoryTableEntity : IDeviceStateHistoryTableEntity
    {
        public DeviceStateHistoryTableEntity(
            string deviceIdentifier,
            string channelName,
            string contactName)
        {
            this.PartitionKey = deviceIdentifier;
            this.RowKey = $"{channelName}-{contactName}";
        }
        
        public string PartitionKey { get; set; }
        
        public string RowKey { get; set; }
        
        public string? ValueSerialized { get; set; }
        
        public DateTime TimeStamp { get; set; }
    }
    
    public interface IDeviceStateTableEntity : ITableEntity
    {
        string DeviceIdentifier { get; set; }
        
        string ChannelName { get; set; }
        
        string ContactName { get; set; }
        
        string? ValueSerialized { get; set; }
        DateTime TimeStamp { get; set; }
    }

    public class DeviceStateTableEntity : IDeviceStateTableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string DeviceIdentifier { get; set; }
        public string ChannelName { get; set; }
        public string ContactName { get; set; }
        public string? ValueSerialized { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}