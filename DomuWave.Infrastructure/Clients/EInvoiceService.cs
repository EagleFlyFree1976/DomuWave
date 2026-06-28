using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using CPQ.Core.Exceptions;
using DomuWave.Services.Clients.EInvoiceProviders;
using DomuWave.Services.Models;

namespace DomuWave.Services.Clients;

/// <summary>
/// Orchestratore del download fatture: valida la configurazione del condominio, risolve
/// il provider SdI corretto (<see cref="IEInvoiceProviderResolver"/>), gli delega il
/// download degli XML grezzi, poi li normalizza con il parser FatturaPA 1.2.1
/// (<see cref="ParseFatturaPa"/>). La logica specifica di ogni provider vive nelle
/// implementazioni di <see cref="IEInvoiceProvider"/>.
/// </summary>
public class EInvoiceService : IEInvoiceService
{
    private readonly IEInvoiceSecretProtector _secretProtector;
    private readonly IEInvoiceProviderResolver _providerResolver;

    public EInvoiceService(
        IEInvoiceSecretProtector secretProtector,
        IEInvoiceProviderResolver providerResolver)
    {
        _secretProtector = secretProtector;
        _providerResolver = providerResolver;
    }

    public async Task<IList<EInvoiceDownloadResult>> DownloadPassiveInvoicesAsync(
        Condominium condominium,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        if (condominium.EInvoiceProviderId is null or 0)
            throw new ValidatorException("Provider fatturazione elettronica non configurato per questo condominio.");

        if (string.IsNullOrWhiteSpace(condominium.EInvoiceApiKey))
            throw new ValidatorException("Chiave API del provider mancante.");

        // P.IVA di ricezione: override specifico se valorizzato, altrimenti quella dell'anagrafica condominio.
        var vatNumber = !string.IsNullOrWhiteSpace(condominium.EInvoiceVatNumber)
            ? condominium.EInvoiceVatNumber
            : condominium.VatNumber;

        if (string.IsNullOrWhiteSpace(vatNumber))
            throw new ValidatorException("Partita IVA mancante: impostala nell'anagrafica del condominio o come override nella configurazione fatture.");

        var apiKey = _secretProtector.Unprotect(condominium.EInvoiceApiKey);

        // Seleziona l'integrazione del provider configurato (Acube/Aruba/Fatture in Cloud).
        var provider = _providerResolver.Resolve(condominium.EInvoiceProviderId.Value);

        var rawDocuments = await provider
            .DownloadAsync(vatNumber, apiKey, from, to, cancellationToken)
            .ConfigureAwait(false);

        if (rawDocuments == null || rawDocuments.Count == 0)
            return new List<EInvoiceDownloadResult>();

        // Parsing centralizzato: stesso FatturaPA per qualunque provider.
        return rawDocuments
            .Select(doc => ParseFatturaPa(doc.Xml, doc.SdiIdentifier))
            .ToList();
    }

    /// <summary>
    /// Estrae i dati rilevanti da un file FatturaPA 1.2.1. Tollerante ai namespace
    /// (li ignora confrontando i soli local name), così funziona sia con XML "p:" sia senza.
    /// </summary>
    public static EInvoiceDownloadResult ParseFatturaPa(string xml, string sdiIdentifier)
    {
        var doc = XDocument.Parse(xml);

        XElement First(XElement parent, string localName) =>
            parent?.Descendants().FirstOrDefault(e => e.Name.LocalName == localName);

        var root = doc.Root;
        var cedente = First(root, "CedentePrestatore");
        var anagrafica = First(cedente, "Anagrafica");
        var idFiscale = First(cedente, "IdFiscaleIVA");
        var datiGenerali = First(root, "DatiGeneraliDocumento");

        // Denominazione oppure Nome + Cognome
        var denominazione = First(anagrafica, "Denominazione")?.Value;
        if (string.IsNullOrWhiteSpace(denominazione))
        {
            var nome = First(anagrafica, "Nome")?.Value;
            var cognome = First(anagrafica, "Cognome")?.Value;
            denominazione = $"{nome} {cognome}".Trim();
        }

        var totale = First(datiGenerali, "ImportoTotaleDocumento")?.Value;
        decimal totalAmount = 0m;
        if (!string.IsNullOrWhiteSpace(totale))
            decimal.TryParse(totale, NumberStyles.Any, CultureInfo.InvariantCulture, out totalAmount);

        var dataStr = First(datiGenerali, "Data")?.Value;
        DateTime invoiceDate = default;
        if (!string.IsNullOrWhiteSpace(dataStr))
            DateTime.TryParse(dataStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out invoiceDate);

        return new EInvoiceDownloadResult
        {
            SdiIdentifier = sdiIdentifier,
            InvoiceNumber = First(datiGenerali, "Numero")?.Value ?? string.Empty,
            InvoiceDate = invoiceDate,
            SupplierVat = First(idFiscale, "IdCodice")?.Value ?? string.Empty,
            SupplierTaxCode = First(cedente, "CodiceFiscale")?.Value ?? string.Empty,
            SupplierName = denominazione ?? string.Empty,
            TotalAmount = totalAmount,
            XmlContent = xml,
        };
    }
}
