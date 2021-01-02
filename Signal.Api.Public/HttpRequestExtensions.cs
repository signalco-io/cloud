using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Signal.Api.Public
{
    public static class HttpRequestExtensions
    {
        public static async Task<T?> ReadAsJsonAsync<T>(this HttpRequest req)
        {
            var requestContent = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(requestContent))
                throw new ExpectedHttpException(HttpStatusCode.BadRequest, "Request empty.");

            return JsonSerializer.Deserialize<T>(
                requestContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}