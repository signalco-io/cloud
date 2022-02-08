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
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Signal.Api.Common;
using Signal.Api.Common.HCaptcha;
using Signal.Core;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Website
{
    public class NewsletterFunction
    {
        private readonly IHCaptchaService hCaptchaService;
        private readonly IAzureStorage storage;
        private readonly ILogger<NewsletterFunction> logger;

        public NewsletterFunction(
            IHCaptchaService hCaptchaService,
            IAzureStorage storage,
            ILogger<NewsletterFunction> logger)
        {
            this.hCaptchaService = hCaptchaService ?? throw new ArgumentNullException(nameof(hCaptchaService));
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [FunctionName("NewsletterFunction")]
        [OpenApiOperation(nameof(NewsletterFunction), "Website", Description = "Subscribe to a newsletter.")]
        [OpenApiParameter(HCaptchaHttpRequestExtensions.HCaptchaHeaderKey, In = ParameterLocation.Header, Description = "hCaptcha response.")]
        [OpenApiRequestBody("application/json", typeof(NewsletterSubscribeDto), Description = "Subscribe with email address.")]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        [OpenApiResponseBadRequestValidation]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "website/newsletter-subscribe")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            await req.VerifyCaptchaAsync(this.hCaptchaService, cancellationToken);
            var data = await req.ReadFromJsonAsync<NewsletterSubscribeDto>(cancellationToken);
            if (string.IsNullOrWhiteSpace(data?.Email))
                return new BadRequestResult();

            // Persist email
            // Don't report errors so bots can't guess-attack
            try
            {
                await storage.CreateOrUpdateItemAsync(
                    ItemTableNames.Website.Newsletter,
                    new NewsletterTableEntity(data.Email.ToUpperInvariant()),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to subscribe to newsletter");
            }

            return new OkResult();
        }

        [Serializable]
        private class NewsletterTableEntity : ITableEntity
        {
            public string PartitionKey => "cover";

            public string RowKey { get; } = Guid.NewGuid().ToString();

            public string Email { get; }

            public NewsletterTableEntity(string email)
            {
                this.Email = email;
            }
        }

        [Serializable]
        private class NewsletterSubscribeDto
        {
            [Required]
            public string? Email { get; set; }
        }
    }
}

