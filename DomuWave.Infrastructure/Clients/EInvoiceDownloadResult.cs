using System;

namespace DomuWave.Services.Clients;

/// <summary>
/// Singola fattura passiva restituita dal provider SdI dopo il download,
/// già normalizzata nei campi che interessano DomuWave. È un DTO di trasporto
/// interno tra <see cref="IEInvoiceService"/> e il consumer di sync.
/// </summary>
public class EInvoiceDownloadResult
{
    /// <summary>Identificativo univoco SdI (chiave di deduplica).</summary>
    public string SdiIdentifier { get; set; } = string.Empty;

    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }

    /// <summary>Partita IVA del cedente/prestatore (fornitore).</summary>
    public string SupplierVat { get; set; } = string.Empty;

    /// <summary>Codice fiscale del cedente/prestatore (quando presente).</summary>
    public string SupplierTaxCode { get; set; } = string.Empty;

    public string SupplierName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }

    /// <summary>XML grezzo della fattura (FatturaPA 1.2.1), conservato integralmente.</summary>
    public string XmlContent { get; set; } = string.Empty;
}
