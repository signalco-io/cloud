using System;
using Signal.Core.Contacts;

namespace Signal.Infrastructure.AzureStorage.Tables;

[Serializable]
internal class AzureContact : AzureTableEntityBase
{
    public string? Name { get; set; }
        
    public string? ValueSerialized { get; set; }

    public DateTime TimeStamp { get; set; }

    public AzureContact() : base(string.Empty, string.Empty)
    {
    }

    protected AzureContact(string partitionKey, string rowKey) : base(partitionKey, rowKey)
    {
    }

    public static AzureContact FromContact(IContact contact)
    {
        return new AzureContact(contact.EntityId, $"{contact.ChannelName}-{contact.ContactName}")
        {
            Name = contact.ContactName,
            ValueSerialized = contact.ValueSerialized,
            TimeStamp = contact.TimeStamp
        };
    }

    public static AzureContact FromContactPointer(IContactPointer contactPointer)
    {
        return new AzureContact(contactPointer.EntityId, $"{contactPointer.ChannelName}-{contactPointer.ContactName}")
        {
            Name = contactPointer.ContactName,
            TimeStamp = DateTime.UtcNow
        };
    }

    public static IContact ToContact(AzureContact contact)
    {
        var channelContactSplit = contact.RowKey.Split("-");
        return new Contact(
            contact.PartitionKey, 
            channelContactSplit[0], 
            channelContactSplit[1],
            contact.ValueSerialized, 
            contact.TimeStamp);
    }
}