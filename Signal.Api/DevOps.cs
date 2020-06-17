using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Signal.Core;
using Signal.Infrastructure.ApiAuth.Oidc.Abstractions;
using Signal.Infrastructure.AzureDevOps;

namespace Signal.Api
{
    public static class DevOps
    {
        [FunctionName("azuredevops-projects-list")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req) =>
            new OkObjectResult(await Class1.GetProjectsAsync());
    }

    public class StorageTableList
    {
        private readonly IApiAuthorization _apiAuthorization;
        private readonly IAzureStorage azureStorage;

        public StorageTableList(
            IApiAuthorization apiAuthorization,
            IAzureStorage azureStorage)
        {
            _apiAuthorization = apiAuthorization ?? throw new System.ArgumentNullException(nameof(apiAuthorization));
            this.azureStorage = azureStorage ?? throw new System.ArgumentNullException(nameof(azureStorage));
        }

        [FunctionName("storage-table-list")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogWarning($"HTTP trigger function {nameof(StorageTableList)} received a request.");

            var authorizationResult = await this._apiAuthorization.AuthorizeAsync(req.Headers);
            if (authorizationResult.Failed)
            {
                log.LogWarning(authorizationResult.FailureReason);
                return new UnauthorizedResult();
            }

            log.LogWarning($"HTTP trigger function {nameof(StorageTableList)} request is authorized.");


            return new OkObjectResult(await this.azureStorage.ListTables());
        }
    }

    public class StorageQueueList
    {
        private readonly IAzureStorage azureStorage;

        public StorageQueueList(IAzureStorage azureStorage)
        {
            this.azureStorage = azureStorage ?? throw new System.ArgumentNullException(nameof(azureStorage));
        }

        [FunctionName("storage-queues-list")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req) =>
            new OkObjectResult(await this.azureStorage.ListQueues());
    }

    public class StorageTableCreate
    {
        private readonly IAzureStorage azureStorage;

        public StorageTableCreate(IAzureStorage azureStorage)
        {
            this.azureStorage = azureStorage ?? throw new System.ArgumentNullException(nameof(azureStorage));
        }

        [FunctionName("storage-table-create")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            using var streamReader = new StreamReader(req.Body);
            var messageContent = await streamReader.ReadToEndAsync();
            var request = JsonConvert.DeserializeAnonymousType(messageContent, new {Name = string.Empty});

            await this.azureStorage.CreateTableAsync(request.Name);
            return new OkObjectResult(await this.azureStorage.ListTables());
        }
    }
}