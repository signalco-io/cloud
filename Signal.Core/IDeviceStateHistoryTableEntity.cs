namespace Signal.Core
{
    public interface IDeviceStateHistoryTableEntity : ITableEntity
    {
        string? ValueSerialized { get; }
    }
}