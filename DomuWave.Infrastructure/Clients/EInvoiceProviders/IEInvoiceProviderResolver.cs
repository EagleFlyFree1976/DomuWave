namespace DomuWave.Services.Clients.EInvoiceProviders;

/// <summary>
/// Seleziona l'implementazione <see cref="IEInvoiceProvider"/> corrispondente
/// all'id di provider configurato sul condominio.
/// </summary>
public interface IEInvoiceProviderResolver
{
    /// <summary>
    /// Restituisce il provider per l'id indicato, o lancia se non è registrato/supportato.
    /// </summary>
    IEInvoiceProvider Resolve(int providerId);
}
