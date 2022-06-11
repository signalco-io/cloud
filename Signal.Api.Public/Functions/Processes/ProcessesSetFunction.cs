using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Signal.Api.Common;
using Signal.Api.Common.Auth;
using Signal.Api.Common.Exceptions;
using Signal.Core.Entities;
using Signal.Core.Exceptions;
using Signal.Infrastructure.AzureStorage.Tables;

namespace Signal.Api.Public.Functions.Processes;

public class ProcessesSetFunction
{
    private readonly IFunctionAuthenticator functionAuthenticator;
    private readonly IEntityService entityService;

    public ProcessesSetFunction(
        IFunctionAuthenticator functionAuthenticator,
        IEntityService entityService)
    {
        this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
        this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
    }

    [FunctionName("Processes-Set")]
    [OpenApiSecurityAuth0Token]
    [OpenApiOperation(nameof(ProcessesSetFunction), "Processes", Description = "Sets the process. This will update or create a process.")]
    [OpenApiRequestBody("application/json", typeof(ProcessSetDto), 
        Description = "Process information. When updating process, Id is required. If Id is not provided, new process will be created.")]
    [OpenApiOkJsonResponse(typeof(ProcessSetResponseDto), Description = "Response containing process ID.")]
    [OpenApiResponseBadRequestValidation]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "processes/set")]
        HttpRequest req,
        CancellationToken cancellationToken) =>
        await req.UserRequest<ProcessSetDto, ProcessSetResponseDto>(cancellationToken, this.functionAuthenticator,
            async context => new ProcessSetResponseDto(await this.entityService.UpsertAsync(
                context.User.UserId,
                context.Payload.Id,
                TableEntityType.Process,
                ItemTableNames.Processes,
                id => new ProcessTableEntity(
                    ProcessType.StateTriggered.ToString().ToLowerInvariant(),
                    id,
                    context.Payload.Alias ?? throw new ExpectedHttpException(HttpStatusCode.BadRequest, "Alias is required"),
                    context.Payload.IsDisabled ?? false,
                    context.Payload.ConfigurationSerialized),
                cancellationToken)));

    private class ProcessSetDto
    {
        public string? Id { get; set;  }

        [Required]
        public string? Alias { get; set; }

        public bool? IsDisabled { get; set;  }

        public string? ConfigurationSerialized { get; set; }
    }

    private class ProcessSetResponseDto
    {
        public string Id { get; }

        public ProcessSetResponseDto(string id)
        {
            this.Id = id;
        }
    }
}