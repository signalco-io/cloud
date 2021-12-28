using Signal.Core;
using Signal.Core.Devices;

namespace Signal.Infrastructure.AzureStorage.Tables;

internal class AzureDeviceStateHistoryTableEntity : AzureTableEntityBase, IDeviceStateHistoryTableEntity
{
    public string PartitionKey { get; set; }

    public string RowKey { get; set; }

    public string? ValueSerialized { get; set; }
}