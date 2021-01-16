using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Signal.Api.Common;
using Signal.Api.Public;
using Signal.Core;

namespace Signal.Api.Internal
{
    public class DevicesStateProcessFunction
    {
        private readonly IAzureStorage azureStorage;

        
        public DevicesStateProcessFunction(
            IAzureStorage azureStorage)
        {
            this.azureStorage = azureStorage ?? throw new ArgumentNullException(nameof(azureStorage));
        }


        [FunctionName("Devices-ProcessState")]
        public async Task Run(
            [QueueTrigger(QueueNames.DevicesState, Connection = SecretKeys.StorageAccountConnectionString)]
            string deviceStateItemSerialized,
            [SignalR(HubName = "devices")] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log, 
            CancellationToken cancellationToken)
        {
            try
            {
                var newState = deviceStateItemSerialized.ToQueueItem<DeviceStateQueueItem>();
                if (newState == null ||
                    string.IsNullOrWhiteSpace(newState.UserId) ||
                    string.IsNullOrWhiteSpace(newState.DeviceId) ||
                    string.IsNullOrWhiteSpace(newState.ChannelName) ||
                    string.IsNullOrWhiteSpace(newState.ContactName))
                    throw new ExpectedHttpException(HttpStatusCode.BadRequest,
                        "State, one or more required state properties are null or empty.");

                // TODO: Validate user assigned
                // TODO: Retrieve device configuration
                // TODO: Validate device has contact for state
                // TODO: Assign to device's ignore states if not assigned in config (update device for state visibility)

                // Persist as current state
                var updateCurrentStateTask = this.azureStorage.CreateOrUpdateItemAsync(
                    ItemTableNames.DeviceStates,
                    new DeviceStateTableEntity(
                        newState.DeviceId,
                        newState.ChannelName,
                        newState.ContactName,
                        newState.ValueSerialized,
                        newState.TimeStamp),
                    cancellationToken);

                // Persist to history 
                // TODO: persist only if given contact is marked for history tracking
                var persistHistoryTask = this.azureStorage.CreateOrUpdateItemAsync(
                    ItemTableNames.DevicesStatesHistory,
                    new DeviceStateHistoryTableEntity(
                        newState.DeviceId,
                        newState.ChannelName,
                        newState.ContactName,
                        newState.ValueSerialized,
                        newState.TimeStamp),
                    cancellationToken);
                
                // Wait for current state update before triggering notification
                await updateCurrentStateTask;

                // Notify listeners
                var notifyStateChangeTask = signalRMessages.AddAsync(new SignalRMessage
                {
                    UserId = newState.UserId,
                    Arguments = new object[]
                    {
                        new SignalDeviceStatePublishDto
                        {
                            DeviceId = newState.DeviceId,
                            ChannelName = newState.ChannelName,
                            ContactName = newState.ContactName,
                            TimeStamp = newState.TimeStamp,
                            ValueSerialized = newState.ValueSerialized
                        }
                    },
                    Target = "devicestate"
                }, cancellationToken);

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