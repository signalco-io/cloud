namespace Signal.Core.Contacts;

public record ContactPointer(string EntityId, string ChannelName, string ContactName) : IContactPointer;