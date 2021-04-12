namespace Signal.Core.Storage
{
    public class TableEntityKey : ITableEntityKey
    {
        public TableEntityKey(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }

        public string PartitionKey { get; }
        
        public string RowKey { get; }
    }
}