using Signal.Core.Devices;

namespace Signal.Infrastructure.AzureStorage.Tables;

internal class AzureDeviceTableEntity : AzureTableEntityBase, IDeviceInfoTableEntity
{
    public string DeviceIdentifier { get; set; }

    public string Alias { get; set; }
}