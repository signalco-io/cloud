using System;
using System.Text.Json.Serialization;

namespace Signal.Api.Common.Entities;

[Serializable]
public class ContactDto
{
    public ContactDto(string entityId, string name, string channel, string? valueSerialized, DateTime timeStamp)
    {
        this.EntityId = entityId;
        this.Name = name;
        this.Channel = channel;
        this.ValueSerialized = valueSerialized;
        this.TimeStamp = timeStamp;
    }

    [JsonPropertyName("entityId")]
    public string EntityId { get; }

    [JsonPropertyName("name")]
    public string Name { get; }

    [JsonPropertyName("channel")]
    public string Channel { get; }

    [JsonPropertyName("valueSerialized")]
    public string? ValueSerialized { get; }

    [JsonPropertyName("timeStamp")]
    public DateTime TimeStamp { get; }
}