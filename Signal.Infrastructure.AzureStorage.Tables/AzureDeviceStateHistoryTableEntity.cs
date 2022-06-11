using Signal.Core.Contacts;

namespace Signal.Infrastructure.AzureStorage.Tables;

internal class AzureDeviceStateHistoryTableEntity : AzureTableEntityBase, IContactHistoryItem
{
    public string PartitionKey { get; set; }

    public string RowKey { get; set; }

    public string? ValueSerialized { get; set; }
}