using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Signal.Api.Public.Auth;
using Signal.Api.Public.Exceptions;
using Signal.Core;
using Signal.Core.Dashboards;
using Signal.Core.Exceptions;

namespace Signal.Api.Public.Functions.Dashboards
{
    public class DashboardsSetFunction
    {
        private readonly IFunctionAuthenticator functionAuthenticator;
        private readonly IEntityService entityService;

        public DashboardsSetFunction(
            IFunctionAuthenticator functionAuthenticator,
            IEntityService entityService)
        {
            this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
            this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        }

        [FunctionName("Dashboards-Set")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "dashboards/set")]
            HttpRequest req,
            CancellationToken cancellationToken) =>
            await req.UserRequest<DashboardsSetRequestDto, DashboardSetResponseDto>(this.functionAuthenticator, async (user, payload) => 
                new DashboardSetResponseDto(await this.entityService.UpsertEntityAsync(
                    user.UserId,
                    payload.Id,
                    ItemTableNames.Dashboards,
                    id => new DashboardTableEntity(
                        id,
                        payload.Name ?? throw new ExpectedHttpException(HttpStatusCode.BadRequest, "Name is required"),
                        payload.ConfigurationSerialized,
                        DateTime.UtcNow),
                    cancellationToken)), cancellationToken);

        [Serializable]
        private class DashboardSetResponseDto
        {
            public DashboardSetResponseDto(string id)
            {
                this.Id = id;
            }

            public string Id { get; }
        }

        [Serializable]
        private class DashboardsSetRequestDto
        {
            public string? Id { get; set;  }

            [Required]
            public string? Name { get; set; }

            public string? ConfigurationSerialized { get; set; }
        }
    }
}