using Signal.Core.Storage;

namespace Signal.Core.Devices;

public class DeviceInfoUpdateTableEntity : ITableEntity
{
    public DeviceInfoUpdateTableEntity(string deviceId, string alias, string? manufacturer, string? model)
    {
        this.PartitionKey = "device";
        this.RowKey = deviceId;
        this.Alias = alias;
        this.Manufacturer = manufacturer;
        this.Model = model;
    }

    public string Alias { get; set; }

    public string? Manufacturer { get; set; }

    public string? Model { get; set; }

    public string PartitionKey { get; }

    public string RowKey { get; }
}