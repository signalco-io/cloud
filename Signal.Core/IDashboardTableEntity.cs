namespace Signal.Core
{
    public interface IDashboardTableEntity : ITableEntity
    {
        public string Name { get; set; }

        public string? ConfigurationSerialized { get; set; }
    }
}