using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;
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
