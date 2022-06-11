using System;

namespace Signal.Core.Contacts;

public interface IContactHistoryItem
{
    IContactPointer ContactPointer { get; }

    string? ValueSerialized { get; }

    DateTimeOffset? Timestamp { get; }
}