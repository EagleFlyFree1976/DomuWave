using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DomuWave.Services.Settings;
using Microsoft.Extensions.Options;

namespace DomuWave.Services.Clients.EInvoiceProviders;

/// <summary>
/// Provider SdI: Fatture in Cloud / TeamSystem (ProviderId = 3).
///
/// STATO: struttura pronta, corpo HTTP da completare (vedi "TODO FattureInCloud").
/// Doc tipica: OAuth2, modello "azienda" da mappare sul condominio.
/// </summary>
public class FattureInCloudInvoiceProvider : IEInvoiceProvider
{
    public int ProviderId => 3;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly EInvoiceSettings _settings;

    public FattureInCloudInvoiceProvider(IHttpClientFactory httpClientFactory, IOptions<EInvoiceSettings> options)
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
        // TODO FattureInCloud: cablare con l'API reale.
        //   1. Auth OAuth2 (apiKey = access token o credenziale secondo setup).
        //   2. Risolvere la company_id corrispondente alla P.IVA del condominio.
        //   3. GET fatture ricevute (received documents) nel periodo + download XML.
        //   4. Restituire List<EInvoiceRawDocument>.
        // ─────────────────────────────────────────────────────────────────────
        _ = (_httpClientFactory, _settings, vatNumber, apiKey, from, to);
        await Task.CompletedTask.ConfigureAwait(false);
        throw new NotImplementedException("Integrazione Fatture in Cloud non ancora configurata.");
    }
}
