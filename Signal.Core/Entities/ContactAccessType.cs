using System;

namespace Signal.Core.Entities;

[Flags]
public enum ContactAccessType
{
    None = 0x0,
    Read = 0x1,
    Write = 0x2,
    Get = 0x4
}