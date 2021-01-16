using System;

namespace Signal.Api.Common
{
    public class SignalDeviceStatePublishDto
    {
        public string? DeviceId { get; set; }

        public string? ChannelName { get; set; }

        public string? ContactName { get; set; }

        public string? ValueSerialized { get; set; }

        public DateTime? TimeStamp { get; set; }
    }
}
