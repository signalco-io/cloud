namespace Signal.Core
{
    public class UserAssignedEntityTableEntry : IUserAssignedEntityTableEntry
    {
        public UserAssignedEntityTableEntry(string userId, string entityId)
        {
            this.PartitionKey = userId;
            this.RowKey = entityId;
        }

        public string PartitionKey { get; }

        public string RowKey { get; }
    }
}