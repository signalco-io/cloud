using System;

namespace Signal.Core
{
    public class DeviceStateQueueItem : IDeviceStateQueueItem
    {
        public DeviceStateQueueItem(Guid id, string userId, DateTime timeStamp, string deviceIdentifier, string channelName, string contactName, string? valueSerialized)
        {
            this.Id = id;
            this.UserId = userId;
            this.TimeStamp = timeStamp;
            this.DeviceIdentifier = deviceIdentifier;
            this.ChannelName = channelName;
            this.ContactName = contactName;
            this.ValueSerialized = valueSerialized;
        }

        public Guid Id { get; set; }
        public string UserId { get; set; }
        public DateTime TimeStamp { get; set; }
        public string DeviceIdentifier { get; set; }
        public string ChannelName { get; set; }
        public string ContactName { get; set; }
        public string? ValueSerialized { get; set; }
    }
}