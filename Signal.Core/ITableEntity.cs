namespace Signal.Core
{
    public interface ITableEntity
    {
        string PartitionKey { get; set; }

        string RowKey { get; set; }
    }
}