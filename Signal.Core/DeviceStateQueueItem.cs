using System;

namespace Signal.Core
{
    public class DeviceStateQueueItem : IDeviceStateQueueItem
    {
        public DeviceStateQueueItem(string userId, string deviceId, string channelName, string contactName, string? valueSerialized, DateTime timeStamp)
        {
            this.UserId = userId;
            this.DeviceId = deviceId;
            this.ChannelName = channelName;
            this.ContactName = contactName;
            this.ValueSerialized = valueSerialized;
            this.TimeStamp = timeStamp;
        }

        public string UserId { get; set; }

        public string DeviceId { get; set; }

        public string ChannelName { get; set; }

        public string ContactName { get; set; }

        public string? ValueSerialized { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}