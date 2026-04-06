using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace DomuWave.IntegrationTests.Infrastructure;

/// <summary>
/// Classe base astratta per tutti i test di integrazione.
///
/// RESPONSABILITÀ:
/// <list type="bullet">
///   <item>Espone un <see cref="Client"/> HTTP preconfigurato e autenticato come <c>DMW_SU</c>
///         (utente di sistema con pieni permessi), pronto per chiamare gli endpoint dell'API.</item>
///   <item>Fornisce helper tipizzati (<c>GetAsync</c>, <c>PostAsync</c>, <c>PutAsync</c>,
///         <c>DeleteAsync</c>) che serializzano/deserializzano automaticamente il JSON
///         e propagano gli errori HTTP.</item>
///   <item>Implementa <see cref="IAsyncLifetime"/> per supportare setup/teardown asincrono
///         tramite <c>InitializeAsync</c> / <c>DisposeAsync</c> (override nelle sottoclassi).</item>
///   <item>Espone <c>AsRole(role)</c> per ottenere un client autenticato con un ruolo diverso
///         (utile per testare la sicurezza e i permessi).</item>
/// </list>
///
/// PATTERN: una singola <see cref="IntegrationTestFactory"/> è condivisa tra tutti i test
/// della stessa <c>[Collection("Integration")]</c>, evitando l'avvio/arresto ripetuto
/// dell'applicazione. I dati creati durante i test vengono rimossi in <c>DisposeAsync</c>
/// da ciascuna sottoclasse (cleanup best-effort con try/catch).
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    /// <summary>
    /// Factory condivisa della Web Application. Contiene l'istanza del server di test
    /// e tutti gli utenti di test configurati.
    /// </summary>
    protected readonly IntegrationTestFactory Factory;

    /// <summary>
    /// Client HTTP predefinito, autenticato come <c>DMW_SU</c> (superuser di sistema).
    /// Contiene gli header <c>Authorization: Bearer {token}</c> e <c>X-Tenant-Id</c>.
    /// </summary>
    protected readonly HttpClient Client;

    /// <summary>Contesto dell'utente di test predefinito (<c>DMW_SU</c>).</summary>
    protected readonly TestUserContext TestUser;

    /// <summary>
    /// Opzioni JSON condivise per la (de)serializzazione delle risposte API.
    /// Insensibile alla maiuscolizzazione delle chiavi e ignora le proprietà null in output.
    /// </summary>
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

    /// <summary>
    /// Restituisce un <see cref="HttpClient"/> autenticato come il ruolo specificato.
    /// Utile per testare endpoint con restrizioni di accesso (es. solo Admin può approvare
    /// un budget, mentre un Condomino può solo visualizzare).
    /// </summary>
    protected HttpClient AsRole(TestRole role) => Factory.CreateClientAs(role);

    // ── IAsyncLifetime ─────────────────────────────────────────────────────────

    /// <summary>
    /// Hook di setup eseguito prima di ogni test. Sovrascrivere nelle sottoclassi
    /// per creare le entità prerequisite (es. Condominio → AnnoFiscale → Budget).
    /// </summary>
    public virtual Task InitializeAsync() => Task.CompletedTask;

    /// <summary>
    /// Hook di teardown eseguito dopo ogni test. Sovrascrivere nelle sottoclassi
    /// per eliminare i dati creati durante il test (cleanup best-effort).
    /// Il <see cref="Client"/> viene smaltito qui.
    /// </summary>
    public virtual Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }

    // ── HTTP helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Esegue una GET sull'<paramref name="url"/> specificato e deserializza la risposta
    /// nel tipo <typeparamref name="T"/>. Lancia un'eccezione se lo status code non è 2xx.
    /// </summary>
    /// <typeparam name="T">Tipo atteso del body della risposta.</typeparam>
    /// <param name="client">Client da usare. Se null, usa il <see cref="Client"/> predefinito.</param>
    protected async Task<T> GetAsync<T>(string url, HttpClient? client = null)
    {
        var response = await (client ?? Client).GetAsync(url);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(JsonOptions))!;
    }

    /// <summary>
    /// Esegue una POST sull'<paramref name="url"/> con il <paramref name="payload"/> serializzato
    /// come JSON. Restituisce sia il <see cref="HttpResponseMessage"/> (per verificare il codice
    /// di stato) sia il body deserializzato nel tipo <typeparamref name="T"/> (se 2xx).
    /// Il body è null se la risposta non è un successo (es. 400, 404).
    /// </summary>
    protected async Task<(HttpResponseMessage Response, T? Body)> PostAsync<T>(string url, object payload, HttpClient? client = null)
    {
        var response = await (client ?? Client).PostAsJsonAsync(url, payload, JsonOptions);
        T? body = default;
        if (response.IsSuccessStatusCode)
            body = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return (response, body);
    }

    /// <summary>
    /// Esegue una PUT sull'<paramref name="url"/> con il <paramref name="payload"/> serializzato.
    /// Restituisce il messaggio di risposta e il body deserializzato (se 2xx).
    /// </summary>
    protected async Task<(HttpResponseMessage Response, T? Body)> PutAsync<T>(string url, object payload, HttpClient? client = null)
    {
        var response = await (client ?? Client).PutAsJsonAsync(url, payload, JsonOptions);
        T? body = default;
        if (response.IsSuccessStatusCode)
            body = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return (response, body);
    }

    /// <summary>
    /// Esegue una DELETE sull'<paramref name="url"/> specificato.
    /// Restituisce il <see cref="HttpResponseMessage"/> grezzo per consentire
    /// l'asserzione sul codice di stato (204 NoContent, 404 NotFound, ecc.).
    /// </summary>
    protected async Task<HttpResponseMessage> DeleteAsync(string url, HttpClient? client = null)
        => await (client ?? Client).DeleteAsync(url);

    /// <summary>
    /// Legge il messaggio di errore dalla risposta HTTP (formato ProblemDetails RFC 7807).
    /// Tenta di estrarre il messaggio nell'ordine: <c>Errors[0]</c> → <c>errors[0]</c>
    /// → <c>detail</c> → <c>title</c> → raw JSON.
    ///
    /// Usato tipicamente per verificare che un 400 Bad Request contenga il messaggio
    /// di validazione atteso (es. "Il codice è già utilizzato").
    /// </summary>
    protected static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Formato DomuWave: array "Errors" (maiuscolo)
            if (root.TryGetProperty("Errors", out var errors) && errors.GetArrayLength() > 0)
                return errors[0].GetString() ?? string.Empty;

            // Formato alternativo: "errors" (minuscolo, standard ASP.NET Core)
            if (root.TryGetProperty("errors", out var errorsLower) && errorsLower.GetArrayLength() > 0)
                return errorsLower[0].GetString() ?? string.Empty;

            // ProblemDetails RFC 7807: "detail"
            if (root.TryGetProperty("detail", out var detail))
                return detail.GetString() ?? string.Empty;

            // Fallback: "title"
            if (root.TryGetProperty("title", out var title))
                return title.GetString() ?? string.Empty;

            return json; // raw JSON come ultimo fallback
        }
        catch
        {
            // Se il parsing JSON fallisce (es. risposta HTML), restituisce il testo grezzo
            return await response.Content.ReadAsStringAsync();
        }
    }
}
