namespace Signal.Core.Devices
{
    public class DeviceTableEntity : DeviceInfoTableEntity, IDeviceTableEntity
    {
        public string? Endpoints { get; set; }

        public DeviceTableEntity(string deviceId, string deviceIdentifier, string alias, string? manufacturer, string? model, string? endpoints) : base(deviceId, deviceIdentifier, alias, manufacturer, model)
        {
            this.Endpoints = endpoints;
        }
    }
}