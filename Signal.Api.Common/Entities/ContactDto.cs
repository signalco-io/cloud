using System;
using System.Text.Json.Serialization;

namespace Signal.Api.Common.Entities;

[Serializable]
public class ContactDto
{
    public ContactDto(string name, string channel, string? valueSerialized, DateTime timeStamp)
    {
        this.Name = name;
        this.Channel = channel;
        this.ValueSerialized = valueSerialized;
        this.TimeStamp = timeStamp;
    }

    [JsonPropertyName("name")]
    public string Name { get; }

    [JsonPropertyName("channel")]
    public string Channel { get; }

    [JsonPropertyName("valueSerialized")]
    public string? ValueSerialized { get; }

    [JsonPropertyName("timeStamp")]
    public DateTime TimeStamp { get; }
}