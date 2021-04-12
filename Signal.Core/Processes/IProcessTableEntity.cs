using Signal.Core.Storage;

namespace Signal.Core.Processes
{
    public interface IProcessTableEntity : ITableEntity
    {
        public string Alias { get; set; }

        public bool IsDisabled { get; set; }

        public string? ConfigurationSerialized { get; set; }
    }
}