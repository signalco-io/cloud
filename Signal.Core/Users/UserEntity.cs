namespace Signal.Core.Users
{
    public class UserEntity : IUserTableEntity
    {
        public UserEntity(string source, string userId, string email, string? fullName)
        {
            this.PartitionKey = source;
            this.RowKey = userId;
            this.Email = email;
            this.FullName = fullName;
        }

        public string PartitionKey { get; }
        public string RowKey { get; }
        public string Email { get; set; }
        public string? FullName { get; set; }
    }
}