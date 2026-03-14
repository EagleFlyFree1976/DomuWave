using DomuWave.Services.Interfaces.FileStorage;

namespace DomuWave.Services.Implementations.FileStorage;

/// <summary>
/// Provider che non gestisce storage esterno: il contenuto e' salvato
/// in DynamicFileContent (base64 su DB) dal consumer.
/// </summary>
public class DatabaseFileStorageProvider : IFileStorageProvider
{
    public int StorageTypeId => 0; // Database

    public Task<string?> SaveAsync(int fileId, string base64Data, string fileName, string contentType, CancellationToken ct)
        => Task.FromResult<string?>(null); // contenuto salvato dal consumer in DynamicFileContent

    public Task<string> ReadAsync(string storageReference, CancellationToken ct)
        => Task.FromResult(string.Empty); // contenuto letto direttamente da DynamicFileContent

    public Task DeleteAsync(string? storageReference, CancellationToken ct)
        => Task.CompletedTask; // nessuna risorsa esterna da eliminare
}
