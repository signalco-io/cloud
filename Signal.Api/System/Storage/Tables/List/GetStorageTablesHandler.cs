using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Signal.Core;
using Voyager.Api;

namespace Signal.Api.System.Storage.Tables.List
{

    public class GetStorageTablesHandler : EndpointHandler<GetStorageTablesRequest, GetStorageTablesResponse>
    {
        private readonly IAzureStorage azureStorage;

        public GetStorageTablesHandler(IAzureStorage azureStorage)
        {
            this.azureStorage = azureStorage ?? throw new ArgumentNullException(nameof(azureStorage));
        }

        public override async Task<ActionResult<GetStorageTablesResponse>> HandleRequestAsync(GetStorageTablesRequest request, CancellationToken cancellationToken)
        {
            var items = await this.azureStorage.ListTables(cancellationToken);
            return new GetStorageTablesResponse
            {
                Items = items.Items
            };
        }
    }
}
