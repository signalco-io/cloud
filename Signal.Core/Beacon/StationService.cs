using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Signal.Core.Beacon;

internal class StationService : IStationService
{
    private readonly ILogger<StationService> logger;


    public StationService(ILogger<StationService> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    public async Task RegisterAsync(string email, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Create station entity
            // TODO: Create station contacts

            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            this.logger.LogWarning(ex, "Failed to register beacon to email {BeaconRegisterEmail}.", email);
        }
    }
}