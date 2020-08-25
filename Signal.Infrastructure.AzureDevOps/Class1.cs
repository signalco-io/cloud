using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Commerce.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.ServiceHooks.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace Signal.Infrastructure.AzureDevOps
{
    public class DevOpsProject
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }

    public class Class1
    {
        private const string azureDevOpsOrganizationUrl = "https://dev.azure.com/dfnoise";
        private const string dfnoiseDevOpsPatFull = "rlajvzgplvt6j74akp2k7m3htvwk2pzrumyohol3x4qqzspdzyfa";

        public static async Task<IEnumerable<DevOpsProject>> GetProjectsAsync()
        {
            var projects = await GetContext(azureDevOpsOrganizationUrl, dfnoiseDevOpsPatFull).Connection.GetClient<ProjectHttpClient>().GetProjects();
            return projects.Select(p => new DevOpsProject {Id = p.Id, Name = p.Name});
        }

        public static async Task CreateSubscriptionAsync()
        {
            var context = GetContext(azureDevOpsOrganizationUrl, dfnoiseDevOpsPatFull);
            var project = await context.Connection
                .GetClient<ProjectHttpClient>().GetProject("5efbd867-e379-42aa-b5f4-801b5141f4af");
            var hooks = context.Connection.GetClient<ServiceHooksManagementHttpClient>();
            await hooks.CreateSubscriptionAsync(new Subscription()
            {
                PublisherId = "tfs",
                EventType = "build.complete",
                ConsumerId = "azureStorageQueue",
                ConsumerActionId = "enqueue",
                ActionDescription = "Account signalstorageaccount, Queue azuredevops"
            });

        }

        private static DevOpsContext GetContext(string organizationUrl, string pat)
        {
            var credentials = new VssBasicCredential("", pat);
            var connection = new VssConnection(new Uri(organizationUrl), credentials);
            return new DevOpsContext(connection);
        }
    }

    public class DevOpsContext
    {
        public DevOpsContext(VssConnection connection)
        {
            this.Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public string OrganizationUrl => this.Connection.Uri.ToString();

        public VssConnection Connection { get; }
    }
}
