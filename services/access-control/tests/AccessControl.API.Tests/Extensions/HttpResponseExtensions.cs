using System.Net.Http.Json;

namespace AccessControl.API.Tests.Extensions;

public static class HttpResponseExtensions
{
    public static async Task<T> ReadAsAsync<T>(this HttpResponseMessage response)
    {
        var result = await response.Content.ReadFromJsonAsync<T>();
        return result ?? throw new InvalidOperationException(
            $"Failed to deserialize response body to {typeof(T).Name}. Status: {response.StatusCode}");
    }
}
