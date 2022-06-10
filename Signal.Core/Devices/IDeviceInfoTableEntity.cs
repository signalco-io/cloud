using Signal.Core.Storage;

namespace Signal.Core.Devices;

public interface IDeviceInfoTableEntity : ITableEntity
{
    public string DeviceIdentifier { get; set; }

    public string Alias { get; set; }
}