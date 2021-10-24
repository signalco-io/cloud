using System;
using Signal.Core;
using Signal.Core.Dashboards;

namespace Signal.Infrastructure.AzureStorage.Tables
{
    internal class AzureDashboardTableEntity : AzureTableEntityBase, IDashboardTableEntity
    {
        public string Name { get; set; }

        public string? ConfigurationSerialized { get; set; }

        public DateTime? TimeStamp { get; set; }
    }
}