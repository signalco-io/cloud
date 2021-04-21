using Signal.Core.Storage;

namespace Signal.Core.Users
{
    public interface IUserTableEntity : ITableEntity
    {
        public string Email { get; set; }

        public string? FullName { get; set; }
    }
}