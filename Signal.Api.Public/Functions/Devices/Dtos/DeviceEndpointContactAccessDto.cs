using System;

namespace Signal.Api.Public.Functions.Devices.Dtos
{
    [Flags]
    public enum DeviceEndpointContactAccessDto
    {
        None = 0x0,
        Read = 0x1,
        Write = 0x2,
        Get = 0x4
    }
}