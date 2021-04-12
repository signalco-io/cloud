using System;
using Signal.Core.Storage;

namespace Signal.Core.Devices
{
    public interface IDeviceStateTableEntity : ITableEntity
    {
        string ChannelName { get; }
        
        string ContactName { get; }
        
        string? ValueSerialized { get; }
        
        DateTime TimeStamp { get; }
    }
}