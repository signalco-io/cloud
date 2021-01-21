namespace Signal.Core
{
    public interface IProcessTableEntity : ITableEntity
    {
        public string Alias { get; set; }

        public bool IsDisabled { get; set; }

        public string? ConfigurationSerialized { get; set; }
    }
}