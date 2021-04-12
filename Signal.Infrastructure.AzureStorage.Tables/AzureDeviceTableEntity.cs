using Signal.Core;
using Signal.Core.Devices;

namespace Signal.Infrastructure.AzureStorage.Tables
{
    internal class AzureDeviceTableEntity : AzureTableEntityBase, IDeviceTableEntity
    {
        public string DeviceIdentifier { get; set; }

        public string Alias { get; set; }
        
        public string? Endpoints { get; set; }
        
        public string? Manufacturer { get; set; }
        
        public string? Model { get; set; }
    }
}