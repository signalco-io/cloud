using Signal.Core.Storage;

namespace Signal.Core.Devices
{
    public interface IDeviceTableEntity : ITableEntity
    {
        public string DeviceIdentifier { get; set; }

        public string Alias { get; set; }
        
        public string? Endpoints { get; set; }
        
        public string? Manufacturer { get; set; }
        
        public string? Model { get; set; }
    }
}