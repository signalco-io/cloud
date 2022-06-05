using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Signal.Api.Common.Conducts
{
    [Serializable]
    public class ConductRequestMultipleDto
    {
        [JsonPropertyName("deviceId")]
        [Required]
        public string? DeviceId { get; set; }

        [JsonPropertyName("channelName")]
        [Required]
        public string? ChannelName { get; set; }

        [JsonPropertyName("contactName")]
        [Required]
        public string? ContactName { get; set; }

        [JsonPropertyName("valueSerialized")]
        public string? ValueSerialized { get; set; }

        [JsonPropertyName("delay")]
        public double? Delay { get; set; }
    }
}
