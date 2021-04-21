using Signal.Core.Dashboards;
using Signal.Core.Users;

namespace Signal.Infrastructure.AzureStorage.Tables
{
    internal class AzureUserTableEntity : AzureTableEntityBase, IUserTableEntity
    {
        public string Email { get; set; }

        public string? FullName { get; set; }
    }
}