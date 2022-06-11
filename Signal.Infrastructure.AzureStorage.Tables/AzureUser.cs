using Signal.Core.Dashboards;
using Signal.Core.Users;

namespace Signal.Infrastructure.AzureStorage.Tables;

internal class AzureUser : AzureTableEntityBase, IUser
{
    public string Email { get; set; }

    public string? FullName { get; set; }
}