namespace DomuWave.Services.Interfaces.FileStorage;

/// <summary>
/// Strategia di storage per i contenuti dei file nel repository dinamico.
/// </summary>
public interface IFileStorageProvider
{
    /// <summary>Id del tipo storage gestito da questo provider (cfr. DynamicFileStorageTypeLookup)</summary>
    int StorageTypeId { get; }

    /// <summary>
    /// Persiste il contenuto base64 e restituisce il riferimento al contenuto (es. path, URL).
    /// Per il provider Database restituisce null — il contenuto viene salvato in DynamicFileContent.
    /// </summary>
    Task<string?> SaveAsync(int fileId, string base64Data, string fileName, string contentType, CancellationToken ct);

    /// <summary>
    /// Legge il contenuto base64 dal riferimento esterno.
    /// Per il provider Database il contenuto viene letto separatamente da DynamicFileContent.
    /// </summary>
    Task<string> ReadAsync(string storageReference, CancellationToken ct);

    /// <summary>Elimina il contenuto esterno associato al riferimento.</summary>
    Task DeleteAsync(string? storageReference, CancellationToken ct);
}
