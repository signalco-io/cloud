using System;

namespace Signal.Core.Contacts;

public class Contact : IContact
{
    public Contact(
        string entityId,
        string channelName,
        string contactName,
        string? valueSerialized,
        DateTime timeStamp)
    {
        EntityId = entityId;
        ChannelName = channelName;
        ContactName = contactName;
        ValueSerialized = valueSerialized;
        TimeStamp = timeStamp;
    }

    public string EntityId { get; }

    public string ChannelName { get; }

    public string ContactName { get; }

    public string? ValueSerialized { get; }

    public DateTime TimeStamp { get; }
}