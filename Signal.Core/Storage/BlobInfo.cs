using System;

namespace Signal.Core.Storage;

public class BlobInfo : IBlobInfo
{
    public string Name { get; init; }
    public DateTimeOffset? CreatedTimeStamp { get; init; }
    public DateTimeOffset? LastModifiedTimeStamp { get; init; }
    public long? Size { get; init; }
}