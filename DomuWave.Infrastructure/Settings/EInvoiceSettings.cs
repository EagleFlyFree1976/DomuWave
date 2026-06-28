namespace DomuWave.Services.Settings;

/// <summary>
/// Impostazioni della piattaforma DomuWave per il download delle fatture elettroniche.
/// Le credenziali del singolo condominio (provider + chiave API + P.IVA) sono memorizzate
/// sull'entità <c>Condominium</c>; qui stanno solo i parametri lato piattaforma.
/// </summary>
public class EInvoiceSettings
{
    /// <summary>Base URL dell'API del provider SdI (es. https://api.provider.it).</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Finestra massima (in giorni) per una singola sincronizzazione, per evitare
    /// download troppo ampi. 0 = nessun limite.
    /// </summary>
    public int MaxSyncWindowDays { get; set; } = 90;
}
