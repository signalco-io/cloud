using Signal.Core.Storage;

namespace Signal.Core.Dashboards
{
    public interface IUserTableEntity : ITableEntity
    {
        public string Email { get; set; }

        public string? FullName { get; set; }
    }
}