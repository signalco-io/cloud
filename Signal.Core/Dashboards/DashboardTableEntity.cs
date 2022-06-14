using System;

namespace Signal.Core.Dashboards;

public class DashboardTableEntity : IDashboardTableEntity
{
    public string PartitionKey { get; }

    public string RowKey { get; }

    public string Name { get; set; }

    public string? ConfigurationSerialized { get; set; }

    public DateTime? TimeStamp { get; }

    public DashboardTableEntity(string id, string name, string? configurationSerialized, DateTime? timeStamp)
    {
        this.PartitionKey = "dashboard";
        this.RowKey = id;
        this.Name = name;
        this.ConfigurationSerialized = configurationSerialized;
        this.TimeStamp = timeStamp;
    }
}