using DomuWave.Services.Interfaces.FileStorage;
using DomuWave.Services.Settings;
using Microsoft.Extensions.Options;

namespace DomuWave.Services.Implementations.FileStorage;

/// <summary>
/// Provider che salva il contenuto su filesystem locale.
/// Il file viene salvato in BasePath/{fileId}_{fileName}.
/// </summary>
public class FileSystemStorageProvider : IFileStorageProvider
{
    private readonly string _basePath;

    public FileSystemStorageProvider(IOptions<DynamicFileSettings> options)
    {
        _basePath = options.Value.FileSystemBasePath ?? string.Empty;
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_basePath))
            throw new InvalidOperationException("DynamicFileSettings.FileSystemBasePath non configurato.");
    }

    public int StorageTypeId => 1; // FileSystem

    public async Task<string?> SaveAsync(int fileId, string base64Data, string fileName, string contentType, CancellationToken ct)
    {
        EnsureConfigured();
        Directory.CreateDirectory(_basePath);

        var safeFileName = $"{fileId}_{Path.GetFileName(fileName)}";
        var fullPath     = Path.Combine(_basePath, safeFileName);

        var bytes = Convert.FromBase64String(base64Data);
        await File.WriteAllBytesAsync(fullPath, bytes, ct).ConfigureAwait(false);

        return fullPath;
    }

    public async Task<string> ReadAsync(string storageReference, CancellationToken ct)
    {
        EnsureConfigured();
        var bytes = await File.ReadAllBytesAsync(storageReference, ct).ConfigureAwait(false);
        return Convert.ToBase64String(bytes);
    }

    public Task DeleteAsync(string? storageReference, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(storageReference) && File.Exists(storageReference))
            File.Delete(storageReference);
        return Task.CompletedTask;
    }
}
