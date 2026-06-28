using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DomuWave.Services.Clients.EInvoiceProviders;

/// <summary>
/// XML grezzo di una fattura ricevuta, così come restituito dal provider SdI,
/// prima del parsing FatturaPA. Il parsing è centralizzato in EInvoiceService.
/// </summary>
public class EInvoiceRawDocument
{
    /// <summary>Identificativo univoco SdI della fattura (chiave di deduplica).</summary>
    public string SdiIdentifier { get; set; } = string.Empty;

    /// <summary>Contenuto XML FatturaPA 1.2.1 della fattura.</summary>
    public string Xml { get; set; } = string.Empty;
}

/// <summary>
/// Integrazione con UN provider SdI accreditato. Ogni implementazione conosce gli
/// endpoint e l'autenticazione del proprio provider; il resolver sceglie quella
/// giusta in base a <see cref="ProviderId"/> configurato sul condominio.
/// </summary>
public interface IEInvoiceProvider
{
    /// <summary>Id del provider, allineato a EInvoiceProviderLookup (1=Acube, 2=Aruba, 3=FattureInCloud).</summary>
    int ProviderId { get; }

    /// <summary>
    /// Scarica gli XML grezzi delle fatture passive ricevute dalla P.IVA indicata
    /// nell'intervallo. <paramref name="apiKey"/> è già in chiaro (decifrata a monte).
    /// </summary>
    Task<IReadOnlyList<EInvoiceRawDocument>> DownloadAsync(
        string vatNumber,
        string apiKey,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
