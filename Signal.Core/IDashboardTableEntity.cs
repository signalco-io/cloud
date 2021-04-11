namespace Signal.Core
{
    public interface IDashboardTableEntity : ITableEntity
    {
        public string? ConfigurationSerialized { get; set; }
    }
}