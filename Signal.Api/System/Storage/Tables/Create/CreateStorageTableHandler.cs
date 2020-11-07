using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Signal.Core;
using Voyager.Api;

namespace Signal.Api.System.Storage.Tables.Create
{
    public class CreateStorageTableHandler : EndpointHandler<CreateStorageTableRequest>
    {
        private readonly IAzureStorage azureStorage;

        public CreateStorageTableHandler(IAzureStorage azureStorage)
        {
            this.azureStorage = azureStorage ?? throw new ArgumentNullException(nameof(azureStorage));
        }
        
        public override async Task<IActionResult> HandleRequestAsync(CreateStorageTableRequest request, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Invalid request.", nameof(request));

            await this.azureStorage.CreateTableAsync(request.Name, cancellation);
            return this.Ok();
        }
    }
}
