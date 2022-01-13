using Signal.Core.Storage;

namespace Signal.Core.Devices
{
    public class DeviceInfoUpdateTableEntity : ITableEntity
    {
        public DeviceInfoUpdateTableEntity(string deviceId, string alias)
        {
            this.PartitionKey = "device";
            this.RowKey = deviceId;
            this.Alias = alias;
        }

        public string Alias { get; set; }

        public string PartitionKey { get; }

        public string RowKey { get; }
    }
}