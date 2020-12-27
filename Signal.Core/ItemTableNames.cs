namespace Signal.Core
{
    public static class ItemTableNames
    {
        public const string Beacons = "beacons";

        public const string DeviceStates = "devicestates";

        public static string DevicesStatesHistory(string userId) => $"devicesstateshistory{userId}";
    }
}