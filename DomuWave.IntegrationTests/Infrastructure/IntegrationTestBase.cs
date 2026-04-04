using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DomuWave.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for all integration test classes.
/// Provides a pre-configured HttpClient and JSON helpers.
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly IntegrationTestFactory Factory;
    protected readonly HttpClient             Client;
    protected readonly TestUserContext        TestUser;

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters                  = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
    };

    protected IntegrationTestBase(IntegrationTestFactory factory)
    {
        Factory  = factory;
        Client   = factory.CreateAuthenticatedClient();
        TestUser = factory.TestUser;
    }

    // ── IAsyncLifetime ────────────────────────────────────────────────────────

    /// <summary>Override to seed test-specific data before each test.</summary>
    public virtual Task InitializeAsync() => Task.CompletedTask;

    /// <summary>Override to clean up test data after each test.</summary>
    public virtual Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }

    // ── HTTP helpers ──────────────────────────────────────────────────────────

    protected async Task<T> GetAsync<T>(string url)
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(JsonOptions))!;
    }

    protected async Task<(HttpResponseMessage Response, T? Body)> PostAsync<T>(string url, object payload)
    {
        var response = await Client.PostAsJsonAsync(url, payload, JsonOptions);
        T? body = default;
        if (response.IsSuccessStatusCode)
            body = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return (response, body);
    }

    protected async Task<(HttpResponseMessage Response, T? Body)> PutAsync<T>(string url, object payload)
    {
        var response = await Client.PutAsJsonAsync(url, payload, JsonOptions);
        T? body = default;
        if (response.IsSuccessStatusCode)
            body = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return (response, body);
    }

    protected async Task<HttpResponseMessage> DeleteAsync(string url)
        => await Client.DeleteAsync(url);

    /// <summary>
    /// Reads the ProblemDetails error response and returns the first error message.
    /// </summary>
    protected static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Try CPQ.Core Errors[] format first
            if (root.TryGetProperty("Errors", out var errors) && errors.GetArrayLength() > 0)
                return errors[0].GetString() ?? string.Empty;

            if (root.TryGetProperty("errors", out var errorsLower) && errorsLower.GetArrayLength() > 0)
                return errorsLower[0].GetString() ?? string.Empty;

            if (root.TryGetProperty("detail", out var detail))
                return detail.GetString() ?? string.Empty;

            if (root.TryGetProperty("title", out var title))
                return title.GetString() ?? string.Empty;

            return json;
        }
        catch
        {
            return await response.Content.ReadAsStringAsync();
        }
    }
}
