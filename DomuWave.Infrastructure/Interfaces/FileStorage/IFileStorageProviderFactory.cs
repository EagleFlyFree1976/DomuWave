namespace DomuWave.Services.Interfaces.FileStorage;

public interface IFileStorageProviderFactory
{
    IFileStorageProvider GetDefault();
    IFileStorageProvider GetById(int storageTypeId);
}
