using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace DomuWave.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for all integration test classes.
/// Provides a pre-configured HttpClient (default: PRT_ADM) and helpers
/// to obtain clients for other roles via AsRole(role).
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly IntegrationTestFactory Factory;

    /// <summary>Default client — authenticated as PRT_ADM.</summary>
    protected readonly HttpClient Client;

    protected readonly TestUserContext TestUser;

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
    };

    protected IntegrationTestBase(IntegrationTestFactory factory)
    {
        Factory  = factory;
        Client   = factory.CreateAuthenticatedClient();
        TestUser = factory.TestUser;
    }

    /// <summary>Returns an HttpClient authenticated as the given role.</summary>
    protected HttpClient AsRole(TestRole role) => Factory.CreateClientAs(role);

    // ── IAsyncLifetime ────────────────────────────────────────────────────────

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }

    // ── HTTP helpers ──────────────────────────────────────────────────────────

    protected async Task<T> GetAsync<T>(string url, HttpClient? client = null)
    {
        var response = await (client ?? Client).GetAsync(url);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(JsonOptions))!;
    }

    protected async Task<(HttpResponseMessage Response, T? Body)> PostAsync<T>(string url, object payload, HttpClient? client = null)
    {
        var response = await (client ?? Client).PostAsJsonAsync(url, payload, JsonOptions);
        T? body = default;
        if (response.IsSuccessStatusCode)
            body = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return (response, body);
    }

    protected async Task<(HttpResponseMessage Response, T? Body)> PutAsync<T>(string url, object payload, HttpClient? client = null)
    {
        var response = await (client ?? Client).PutAsJsonAsync(url, payload, JsonOptions);
        T? body = default;
        if (response.IsSuccessStatusCode)
            body = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return (response, body);
    }

    protected async Task<HttpResponseMessage> DeleteAsync(string url, HttpClient? client = null)
        => await (client ?? Client).DeleteAsync(url);

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
