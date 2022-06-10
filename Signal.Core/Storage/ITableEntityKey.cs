namespace Signal.Core.Storage;

public interface ITableEntityKey
{
    string PartitionKey { get; }

    string RowKey { get; }
}