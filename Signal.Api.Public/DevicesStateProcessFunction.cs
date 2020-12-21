using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Signal.Core;

namespace Signal.Api.Public
{
    public class DevicesStateProcessFunction
    {
        private readonly IAzureStorage azureStorage;
        private readonly IAzureStorageDao azureStorageDao;

        
        public DevicesStateProcessFunction(
            IAzureStorage azureStorage,
            IAzureStorageDao azureStorageDao)
        {
            this.azureStorage = azureStorage ?? throw new ArgumentNullException(nameof(azureStorage));
            this.azureStorageDao = azureStorageDao ?? throw new ArgumentNullException(nameof(azureStorageDao));
        }
        
        
        [FunctionName("Devices-ProcessState")]
        public async Task Run([QueueTrigger(QueueNames.DevicesState, Connection = SecretKeys.StorageAccountConnectionString)] string deviceStateItemSerialized, ILogger log, CancellationToken cancellationToken)
        {
            log.LogDebug("Processing state {State}", deviceStateItemSerialized);

            var state = deviceStateItemSerialized.ToQueueItem<DeviceStateQueueItem>();

            // TODO: Retrieve device configuration (validate user assigned)
            // TODO: Validate device has contact for state
            // TODO: Assigned to ignores states if not assigned in config

            // Retrieve current state
            //var currentState = await this.azureStorageDao.GetDeviceStateAsync(
            //    new TableEntityKey(state.UserId, $"{state.DeviceIdentifier}-{state.ChannelName}-{state.ContactName}"),
            //    cancellationToken);
            
            // TODO: Discard if outdated

            // TODO: Persist as current state (parallel)
            await this.azureStorage.CreateOrUpdateItemAsync("devicestates", new DeviceStateTableEntity()
            {
                PartitionKey = state.UserId,
                RowKey = $"{state.DeviceIdentifier}-{state.ChannelName}-{state.ContactName}",
                DeviceIdentifier = state.DeviceIdentifier,
                ChannelName = state.ChannelName,
                ContactName = state.ContactName,
                ValueSerialized = state.ValueSerialized
            }, cancellationToken);

            // TODO: Persist to history (if tracker for given contact) (parallel)
            // TODO: Notify listeners (parallel)
        }
    }
}