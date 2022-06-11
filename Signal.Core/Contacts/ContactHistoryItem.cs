using System;

namespace Signal.Core.Contacts;

public class ContactHistoryItem : IContactHistoryItem
{
    public ContactHistoryItem(
        IContactPointer contactPointer,
        string? valueSerialized,
        DateTime timeStamp)
    {
        ContactPointer = contactPointer;
        ValueSerialized = valueSerialized;
        Timestamp = timeStamp;
    }

    public IContactPointer ContactPointer { get; }

    public string? ValueSerialized { get; }

    public DateTimeOffset? Timestamp { get; }
}