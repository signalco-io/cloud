namespace Signal.Core
{
    public class DashboardTableEntity : IDashboardTableEntity
    {
        public string PartitionKey { get; }

        public string RowKey { get; }

        public string Name { get; set; }

        public string? ConfigurationSerialized { get; set; }

        public DashboardTableEntity(string id, string name, string? configurationSerialized)
        {
            this.PartitionKey = "dashboard";
            this.RowKey = id;
            this.Name = name;
            this.ConfigurationSerialized = configurationSerialized;
        }
    }
}