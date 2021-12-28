using System;

namespace Signal.Infrastructure.AzureStorage.Tables;

internal class AzureBeaconTableEntity : AzureTableEntityBase
{
    public DateTime RegisteredTimeStamp { get; set; }

    public string? Version { get; set; }

    public DateTime? StateTimeStamp { get; set; }

    public string? AvailableWorkerServices { get; set; }

    public string? RunningWorkerServices { get; set; }
}