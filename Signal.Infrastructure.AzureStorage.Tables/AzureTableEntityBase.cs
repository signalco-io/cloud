using System;
using Azure;
using Azure.Data.Tables;

namespace Signal.Infrastructure.AzureStorage.Tables;

internal abstract class AzureTableEntityBase : ITableEntity
{
    protected AzureTableEntityBase(string partitionKey, string rowKey)
    {
        PartitionKey = partitionKey;
        RowKey = rowKey;
    }

    public string PartitionKey { get; set; }
        
    public string RowKey { get; set; }
        
    public DateTimeOffset? Timestamp { get; set; }
        
    public ETag ETag { get; set; }
}