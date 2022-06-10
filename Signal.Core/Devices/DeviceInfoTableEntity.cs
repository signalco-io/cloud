namespace Signal.Core.Devices;

public class DeviceInfoTableEntity : IDeviceInfoTableEntity
{
    public DeviceInfoTableEntity(string deviceId, string deviceIdentifier, string alias, string? manufacturer, string? model)
    {
        this.PartitionKey = "device";
        this.RowKey = deviceId;
        this.DeviceIdentifier = deviceIdentifier;
        this.Alias = alias;
        this.Manufacturer = manufacturer;
        this.Model = model;
    }

    public string DeviceIdentifier { get; set; }

    public string Alias { get; set; }

    public string? Manufacturer { get; set; }

    public string? Model { get; set; }
        
    public string PartitionKey { get; }

    public string RowKey { get; }
}