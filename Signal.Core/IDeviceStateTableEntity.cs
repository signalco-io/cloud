using System;

namespace Signal.Core
{
    public interface IDeviceStateTableEntity : ITableEntity
    {
        string ChannelName { get; }
        
        string ContactName { get; }
        
        string? ValueSerialized { get; }
        
        DateTime TimeStamp { get; }
    }
}