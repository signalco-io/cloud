using System;

namespace Signal.Core
{
    public static class ItemTableNames
    {
        //public const string Users = "users";

        public static string UserAssignedEntity(EntityType type) =>
            type switch
            {
                EntityType.Device => "userassigneddevices",
                EntityType.Process => "userassignedprocesses",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

        public const string Beacons = "beacons";

        public const string Devices = "devices";

        public const string DeviceStates = "devicestates";

        public const string DevicesStatesHistory = "devicesstateshistory";

        public const string Processes = "processes";
    }
}