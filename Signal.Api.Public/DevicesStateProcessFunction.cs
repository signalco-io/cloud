using System;
using System.Net;
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
        public async Task Run(
            [QueueTrigger(QueueNames.DevicesState, Connection = SecretKeys.StorageAccountConnectionString)]
            string deviceStateItemSerialized, ILogger log, CancellationToken cancellationToken)
        {
            try
            {
                var newState = deviceStateItemSerialized.ToQueueItem<DeviceStateQueueItem>();
                if (newState == null ||
                    string.IsNullOrWhiteSpace(newState.UserId) ||
                    string.IsNullOrWhiteSpace(newState.DeviceIdentifier) ||
                    string.IsNullOrWhiteSpace(newState.ChannelName) ||
                    string.IsNullOrWhiteSpace(newState.ContactName))
                    throw new ExpectedHttpException(HttpStatusCode.BadRequest,
                        "State, one or more required state properties are null or empty.");

                // TODO: Retrieve device configuration (validate user assigned)
                // TODO: Validate device has contact for state
                // TODO: Assign to device's ignore states if not assigned in config (update device for state visibility)

                // Retrieve current state
                var currentState = await this.azureStorageDao.GetDeviceStateAsync(
                    new TableEntityKey(newState.UserId,
                        $"{newState.DeviceIdentifier}-{newState.ChannelName}-{newState.ContactName}"),
                    cancellationToken);

                // Ignore outdated states
                if (newState.TimeStamp < currentState.TimeStamp)
                    return;

                // Persist as current state
                var updateCurrentStateTask = this.azureStorage.CreateOrUpdateItemAsync("devicestates",
                    new DeviceStateTableEntity
                    {
                        PartitionKey = newState.UserId,
                        RowKey = $"{newState.DeviceIdentifier}-{newState.ChannelName}-{newState.ContactName}",
                        DeviceIdentifier = newState.DeviceIdentifier,
                        ChannelName = newState.ChannelName,
                        ContactName = newState.ContactName,
                        ValueSerialized = newState.ValueSerialized,
                        TimeStamp = newState.TimeStamp
                    }, cancellationToken);

                // Persist to history 
                // TODO: persist only if given contact is marked for history tracking
                var historyTableName = $"devicesstateshistory-{newState.UserId}";
                await this.azureStorage.EnsureTableExistsAsync(historyTableName, cancellationToken);
                var persistHistoryTask = this.azureStorage.CreateOrUpdateItemAsync(
                    historyTableName,
                    new DeviceStateHistoryTableEntity(newState.DeviceIdentifier, newState.ChannelName,
                        newState.ContactName)
                    {
                        ValueSerialized = newState.ValueSerialized,
                        TimeStamp = newState.TimeStamp
                    },
                    cancellationToken);

                // Wait for current state update before triggering notification
                await updateCurrentStateTask;

                // TODO: Notify listeners
                var notifyStateChangeTask = Task.CompletedTask;

                // Wait for all to finish
                await Task.WhenAll(
                    notifyStateChangeTask,
                    persistHistoryTask);
            }
            catch(Exception ex)
            {
                log.LogError(ex, "Failed to process state.");
                throw;
            }
        }
    }
}