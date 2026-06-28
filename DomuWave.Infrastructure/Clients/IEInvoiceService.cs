using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Clients;

/// <summary>
/// Wrapper sull'API di un provider SdI accreditato per il download massivo delle fatture
/// elettroniche passive del condominio dal Cassetto Fiscale. La configurazione (provider,
/// chiave API, P.IVA) è per-condominio: la delega all'intermediario è gestita fuori app.
/// </summary>
public interface IEInvoiceService
{
    /// <summary>
    /// Scarica dal provider tutte le fatture passive del condominio ricevute nell'intervallo
    /// indicato e le restituisce normalizzate. Non persiste nulla: la deduplica e il salvataggio
    /// sono responsabilità del consumer. Le credenziali sono lette da <paramref name="condominium"/>
    /// (<c>EInvoiceProviderId</c>, <c>EInvoiceApiKey</c> decifrata, <c>EInvoiceVatNumber</c>).
    /// </summary>
    Task<IList<EInvoiceDownloadResult>> DownloadPassiveInvoicesAsync(
        Condominium condominium,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
