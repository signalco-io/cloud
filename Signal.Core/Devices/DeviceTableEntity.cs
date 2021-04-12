namespace Signal.Core.Devices
{
    public class DeviceTableEntity : IDeviceTableEntity
    {
        public string PartitionKey { get; }
        
        public string RowKey { get; }
        
        public string DeviceIdentifier { get; set; }
        
        public string Alias { get; set; }
        
        public string? Endpoints { get; set; }
        
        public string? Manufacturer { get; set; }
        
        public string? Model { get; set; }

        public DeviceTableEntity(string deviceId, string deviceIdentifier, string alias)
        {
            this.PartitionKey = "device";
            this.RowKey = deviceId;
            this.DeviceIdentifier = deviceIdentifier;
            this.Alias = alias;
        }
    }
}