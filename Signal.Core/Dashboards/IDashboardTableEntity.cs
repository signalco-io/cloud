using Signal.Core.Storage;

namespace Signal.Core.Dashboards
{
    public interface IDashboardTableEntity : ITableEntity
    {
        public string Name { get; set; }

        public string? ConfigurationSerialized { get; set; }
    }
}