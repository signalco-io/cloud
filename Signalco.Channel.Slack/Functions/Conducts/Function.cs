using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Signal.Api.Common;
using Signal.Api.Common.Auth;
using Signal.Api.Common.Conducts;
using Signal.Api.Common.Exceptions;
using Signal.Core;

namespace Signalco.Channel.Slack.Functions.Conducts
{
    public class ConductRequestMultipleFunction
    {
        private readonly IFunctionAuthenticator authenticator;
        private readonly IEntityService entityService;

        public ConductRequestMultipleFunction(
            IFunctionAuthenticator authenticator,
            IEntityService entityService)
        {
            this.authenticator = authenticator ?? throw new ArgumentNullException(nameof(authenticator));
            this.entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        }

        [FunctionName("Conducts-RequestMultiple")]
        [OpenApiSecurityAuth0Token]
        [OpenApiOperation(nameof(ConductRequestMultipleFunction), "Conducts",
            Description = "Requests multiple conducts to be executed.")]
        [OpenApiRequestBody("application/json", typeof(List<ConductRequestMultipleDto>),
            Description = "Collection of conducts to execute.")]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        [OpenApiResponseBadRequestValidation]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "conducts/request-multiple")]
            HttpRequest req,
            CancellationToken cancellationToken) =>
            await req.UserRequest<List<ConductRequestMultipleDto>>(cancellationToken, this.authenticator,
                async context =>
                {
                    var payload = context.Payload;
                    throw new NotImplementedException();
                    //this.entityService.GetAsync()
                });
    }
}
