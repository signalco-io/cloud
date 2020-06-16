using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using Signal.Infrastructure.AzureDevOps;
using Signal.Infrastructure.AzureStorage.Tables;

namespace Signal.Api
{
    public static class DevOps
    {
        [FunctionName("azuredevops-projects-list")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req) =>
            new OkObjectResult(await Class1.GetProjectsAsync());
    }

    public static class StorageTableList
    {
        [FunctionName("storage-table-list")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req) =>
            new OkObjectResult(await AzureStorage.ListTables());
    }

    public static class StorageQueueList
    {
        [FunctionName("storage-queues-list")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req) =>
            new OkObjectResult(await AzureStorage.ListQueues());
    }

    public static class StorageTableCreate
    {
        [FunctionName("storage-table-create")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            using var streamReader = new StreamReader(req.Body);
            var messageContent = await streamReader.ReadToEndAsync();
            var request = JsonConvert.DeserializeAnonymousType(messageContent, new {Name = string.Empty});

            await AzureStorage.CreateTableAsync(request.Name);
            return new OkObjectResult(await AzureStorage.ListTables());
        }
    }
}