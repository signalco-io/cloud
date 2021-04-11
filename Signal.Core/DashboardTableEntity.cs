namespace Signal.Core
{
    public class DashboardTableEntity : IDashboardTableEntity
    {
        public string PartitionKey { get; }

        public string RowKey { get; }

        public string? ConfigurationSerialized { get; set; }

        public DashboardTableEntity(string id, string name, string? configurationSerialized)
        {
            this.PartitionKey = id;
            this.RowKey = name;
            this.ConfigurationSerialized = configurationSerialized;
        }
    }
}