namespace Signal.Core.Processes;

public class ProcessTableEntity : IProcessTableEntity
{
    public string PartitionKey { get; }
        
    public string RowKey { get; }
        
    public string Alias { get; set; }
        
    public bool IsDisabled { get; set; }
        
    public string? ConfigurationSerialized { get; set; }

    public ProcessTableEntity(string type, string id, string alias, bool isDisabled, string? configurationSerialized)
    {
        this.PartitionKey = type;
        this.RowKey = id;
        this.Alias = alias;
        this.IsDisabled = isDisabled;
        this.ConfigurationSerialized = configurationSerialized;
    }
}