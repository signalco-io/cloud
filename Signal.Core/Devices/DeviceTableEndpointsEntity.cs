using Signal.Core.Storage;

namespace Signal.Core.Devices
{
    public class DeviceTableEndpointsEntity : ITableEntity
    {
        public string PartitionKey { get; }
        
        public string RowKey { get; }
        
        public string Endpoints { get; set; }

        public DeviceTableEndpointsEntity(string deviceId, string endpoints)
        {
            this.PartitionKey = "device";
            this.RowKey = deviceId;
            this.Endpoints = endpoints;
        }
    }
}