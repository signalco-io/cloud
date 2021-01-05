using System;

namespace Signal.Core
{
    public interface IDeviceStateQueueItem : IQueueItem
    {
        public string UserId { get; set; }

        public string DeviceId { get; set; }

        public string ChannelName { get; set; }
        
        public string ContactName { get; set; }
        
        public string? ValueSerialized { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}