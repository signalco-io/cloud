using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Signal.Infrastructure.AzureDevOps;
using Signal.Infrastructure.AzureStorage.Tables;

namespace Signal.Api
{
    public static class DevOps
    {
        [FunctionName("azuredevops-projects-list")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            var data = await Class1.GetProjectsAsync();

            return new OkObjectResult(data);
        }
    }

    public static class StorageTableList
    {
        [FunctionName("storage-table-list")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            return new OkObjectResult(await AzureStorage.ListTables());
        }
    }

    public static class StorageTableCreate
    {
        [FunctionName("storage-table-create")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            var tableName = req.GetQueryParameterDictionary()["name"];
            await AzureStorage.CreateTableAsync(tableName);
            return new OkObjectResult(await AzureStorage.ListTables());
        }
    }
}