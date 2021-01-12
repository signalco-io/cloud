namespace Signal.Core
{
    public interface ITableEntityKey
    {
        string PartitionKey { get; }

        string RowKey { get; }
    }
}