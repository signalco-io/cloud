using System;
using Azure;
using Azure.Data.Tables;

namespace Signal.Infrastructure.AzureStorage.Tables
{
    internal class AzureTableEntityBase : ITableEntity
    {
        public string PartitionKey { get; set; }
        
        public string RowKey { get; set; }
        
        public DateTimeOffset? Timestamp { get; set; }
        
        public ETag ETag { get; set; }
    }
}