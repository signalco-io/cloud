using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Signal.Core.Storage;

namespace Signal.Core.Beacon
{
    public interface IBeaconService
    {
        Task RegisterAsync(string email, CancellationToken cancellationToken);
    }

    internal class BeaconService : IBeaconService
    {
        private readonly ILogger<BeaconService> logger;


        public BeaconService(ILogger<BeaconService> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task RegisterAsync(string email, CancellationToken cancellationToken)
        {
            try
            {
                // TODO: Add to beacons table

                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(ex, "Failed to register beacon to email {BeaconRegisterEmail}.", email);
            }
        }
    }

    public interface IBeaconRegistrationTableEntity : ITableEntity
    {
        DateTime? RequestDateTime { get; set; }
    }

    public interface IBeaconTableEntity : ITableEntity
    {
    }
}
