using System;
using Signal.Core.Storage;

namespace Signal.Core;

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
    
    public const string Users = "users";

    public static class Website
    {
        public const string Newsletter = "webnewsletter";
    }
}