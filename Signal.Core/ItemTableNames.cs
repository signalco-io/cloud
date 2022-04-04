using System;
using Signal.Core.Storage;

namespace Signal.Core
{
    public static class ItemTableNames
    {
        public static string UserAssignedEntity(TableEntityType type) =>
            type switch
            {
                TableEntityType.Device => "userassigneddevices",
                TableEntityType.Process => "userassignedprocesses",
                TableEntityType.Dashboard => "userassigneddashboards",
                TableEntityType.Station => "userassignedbeacons",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

        public const string Beacons = "beacons";

        public const string Devices = "devices";

        public const string DeviceStates = "devicestates";

        public const string DevicesStatesHistory = "devicesstateshistory";

        public const string Processes = "processes";

        public const string Dashboards = "dashboards";

        public const string Users = "users";

        public static class Website
        {
            public const string Newsletter = "webnewsletter";
        }
    }
}