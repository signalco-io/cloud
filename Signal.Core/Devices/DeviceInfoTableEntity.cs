namespace Signal.Core.Devices
{
    public class DeviceInfoTableEntity : IDeviceInfoTableEntity
    {
        public DeviceInfoTableEntity(string deviceId, string deviceIdentifier, string alias)
        {
            this.PartitionKey = "device";
            this.RowKey = deviceId;
            this.DeviceIdentifier = deviceIdentifier;
            this.Alias = alias;
        }

        public string DeviceIdentifier { get; set; }

        public string Alias { get; set; }
        
        public string PartitionKey { get; }

        public string RowKey { get; }
    }
}