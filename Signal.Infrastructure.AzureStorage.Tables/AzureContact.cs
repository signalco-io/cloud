using System;
using Signal.Core;
using Signal.Core.Contacts;

namespace Signal.Infrastructure.AzureStorage.Tables;

internal class AzureContact : AzureTableEntityBase, IContact
{
    public string DeviceIdentifier { get; set; }
        
    public string ChannelName { get; set; }
        
    public string ContactName { get; set; }
        
    public string? ValueSerialized { get; set; }

    public DateTime TimeStamp { get; set; }
}