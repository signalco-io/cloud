namespace Signal.Core.Contacts;

public interface IContactPointer
{
    string EntityId { get; init; }
    string ChannelName { get; init; }
    string ContactName { get; init; }

    string ToString();
}