using System;
using System.ComponentModel.DataAnnotations;

namespace Signal.Api.Common.Conducts
{
    [Serializable]
    public class ConductRequestMultipleDto
    {
        [Required]
        public string? DeviceId { get; set; }

        [Required]
        public string? ChannelName { get; set; }

        [Required]
        public string? ContactName { get; set; }

        public string? ValueSerialized { get; set; }

        public double? Delay { get; set; }
    }
}
