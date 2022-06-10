namespace Signal.Core.Devices;

public interface IDeviceTableEntity : IDeviceInfoTableEntity
{
    public string? Endpoints { get; set; }
}