using System;
using Signal.Core.Storage;

namespace Signal.Core.Devices
{
    public interface IDeviceStateHistoryTableEntity : ITableEntity
    {
        string? ValueSerialized { get; }

        DateTimeOffset? Timestamp { get; }
    }
}