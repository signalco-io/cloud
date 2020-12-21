using System.Text.Json;

namespace Signal.Core
{
    public static class IQueueItemExtensions
    {
        public static T? ToQueueItem<T>(this string @data)
            where T : class =>
            JsonSerializer.Deserialize<T>(@data, new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
    }
}