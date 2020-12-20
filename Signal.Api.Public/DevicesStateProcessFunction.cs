using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Signal.Core;

namespace Signal.Api.Public
{
    public class DevicesStateProcessFunction
    {
        [FunctionName("Devices-ProcessState")]
        public void Run([QueueTrigger(QueueNames.DevicesState, Connection = SecretKeys.StorageAccountConnectionString)] string deviceStateItemSerialized, ILogger log)
        {
            log.LogDebug("Processing state {State}", deviceStateItemSerialized);

            //var state = deviceStateItemSerialized.ToQueueItem<DeviceStateQueueItem>();

            // TODO: Retrieve device configuration (validate user assigned)
            // TODO: Validate device has contact for state
            // TODO: Assigned to ignores states if not assigned in config

            // TODO: Retrieve current state
            // TODO: Discard if outdated

            // TODO: Persist as current state (parallel)
            // TODO: Persist to history (if tracker for given contact) (parallel)
            // TODO: Notify listeners (parallel)
        }
    }
}