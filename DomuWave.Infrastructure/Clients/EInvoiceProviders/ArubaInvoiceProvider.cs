using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DomuWave.Services.Settings;
using Microsoft.Extensions.Options;

namespace DomuWave.Services.Clients.EInvoiceProviders;

/// <summary>
/// Provider SdI: Aruba (ProviderId = 2).
///
/// STATO: struttura pronta, corpo HTTP da completare (vedi "TODO Aruba").
/// Doc tipica: autenticazione a token, API fatture ricevute.
/// </summary>
public class ArubaInvoiceProvider : IEInvoiceProvider
{
    public int ProviderId => 2;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly EInvoiceSettings _settings;

    public ArubaInvoiceProvider(IHttpClientFactory httpClientFactory, IOptions<EInvoiceSettings> options)
    {
        _httpClientFactory = httpClientFactory;
        _settings = options.Value;
    }

    public async Task<IReadOnlyList<EInvoiceRawDocument>> DownloadAsync(
        string vatNumber,
        string apiKey,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        // ─────────────────────────────────────────────────────────────────────
        // TODO Aruba: cablare con l'API reale.
        //   1. Autenticazione (token) secondo la doc Aruba.
        //   2. GET fatture ricevute: cessionario = vatNumber, data in [from, to].
        //   3. Download XML grezzo + id SdI per ciascuna.
        //   4. Restituire List<EInvoiceRawDocument>.
        // ─────────────────────────────────────────────────────────────────────
        _ = (_httpClientFactory, _settings, vatNumber, apiKey, from, to);
        await Task.CompletedTask.ConfigureAwait(false);
        throw new NotImplementedException("Integrazione Aruba non ancora configurata.");
    }
}
