using System;

namespace Signal.Core.Storage;

public interface IBlobInfo
{
    string Name { get; }
        
    DateTimeOffset? CreatedTimeStamp { get; }
        
    DateTimeOffset? LastModifiedTimeStamp { get; }

    long? Size { get; }
}