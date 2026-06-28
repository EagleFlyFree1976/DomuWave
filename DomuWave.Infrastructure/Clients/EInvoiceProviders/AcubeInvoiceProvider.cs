using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DomuWave.Services.Settings;
using Microsoft.Extensions.Options;

namespace DomuWave.Services.Clients.EInvoiceProviders;

/// <summary>
/// Provider SdI: Acube / A-Cube (ProviderId = 1).
///
/// STATO: struttura pronta, corpo HTTP da completare (vedi "TODO Acube").
/// Doc tipica: auth Bearer/OAuth, endpoint per fatture ricevute + download XML.
/// </summary>
public class AcubeInvoiceProvider : IEInvoiceProvider
{
    public int ProviderId => 1;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly EInvoiceSettings _settings;

    public AcubeInvoiceProvider(IHttpClientFactory httpClientFactory, IOptions<EInvoiceSettings> options)
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
        // TODO Acube: cablare con l'API reale.
        //   var client = _httpClientFactory.CreateClient("EInvoice");
        //   client.BaseAddress = new Uri(_settings.BaseUrl);
        //   client.DefaultRequestHeaders.Authorization =
        //       new AuthenticationHeaderValue("Bearer", apiKey);
        //
        //   1. GET fatture ricevute: filtro cessionario = vatNumber, data in [from, to].
        //   2. Per ciascuna: GET dell'XML grezzo + relativo id SdI.
        //   3. Restituire List<EInvoiceRawDocument> { SdiIdentifier, Xml }.
        // ─────────────────────────────────────────────────────────────────────
        _ = (_httpClientFactory, _settings, vatNumber, apiKey, from, to);
        await Task.CompletedTask.ConfigureAwait(false);
        throw new NotImplementedException("Integrazione Acube non ancora configurata.");
    }
}
