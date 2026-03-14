using DomuWave.Services.Interfaces.FileStorage;
using DomuWave.Services.Settings;
using Microsoft.Extensions.Options;

namespace DomuWave.Services.Implementations.FileStorage;

public class FileStorageProviderFactory : IFileStorageProviderFactory
{
    private readonly IEnumerable<IFileStorageProvider> _providers;
    private readonly int _defaultStorageTypeId;

    public FileStorageProviderFactory(
        IEnumerable<IFileStorageProvider> providers,
        IOptions<DynamicFileSettings> options)
    {
        _providers           = providers;
        _defaultStorageTypeId = options.Value.DefaultStorageTypeId;
    }

    public IFileStorageProvider GetDefault()
        => GetById(_defaultStorageTypeId);

    public IFileStorageProvider GetById(int storageTypeId)
    {
        var provider = _providers.FirstOrDefault(p => p.StorageTypeId == storageTypeId);
        if (provider == null)
            throw new InvalidOperationException($"Nessun provider configurato per StorageTypeId={storageTypeId}.");
        return provider;
    }
}
