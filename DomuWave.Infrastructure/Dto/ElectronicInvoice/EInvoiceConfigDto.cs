using System;

namespace DomuWave.Services.Dto.ElectronicInvoice;

/// <summary>
/// Configurazione corrente del download fatture elettroniche per un condominio.
/// Non espone mai la chiave API: indica solo se è impostata (<see cref="HasApiKey"/>).
/// </summary>
public class EInvoiceConfigReadDto
{
    public int CondominiumId { get; set; }

    /// <summary>Provider SdI selezionato (EInvoiceProviderLookup): 0/null = non configurato.</summary>
    public int? ProviderId { get; set; }
    public string ProviderName { get; set; }

    /// <summary>Override opzionale della P.IVA di ricezione (vuoto = usa quella dell'anagrafica).</summary>
    public string VatNumberOverride { get; set; }

    /// <summary>P.IVA dell'anagrafica condominio (default di ricezione).</summary>
    public string CondominiumVatNumber { get; set; }

    /// <summary>P.IVA effettivamente usata per il download (override se presente, altrimenti anagrafica).</summary>
    public string EffectiveVatNumber { get; set; }

    /// <summary>true se una chiave API è già memorizzata (il valore non viene mai restituito).</summary>
    public bool HasApiKey { get; set; }

    public DateTime? LastSyncDate { get; set; }
}

/// <summary>
/// Dati di configurazione inviati dall'amministratore. La chiave API è opzionale in
/// update: se null/vuota la chiave esistente resta invariata (così non va re-inserita).
/// </summary>
public class EInvoiceConfigUpdateDto
{
    public int? ProviderId { get; set; }

    /// <summary>Override opzionale della P.IVA di ricezione. Vuoto = usa quella dell'anagrafica condominio.</summary>
    public string VatNumberOverride { get; set; }

    /// <summary>Nuova chiave API. Null/vuota = lascia invariata quella già salvata.</summary>
    public string ApiKey { get; set; }
}
