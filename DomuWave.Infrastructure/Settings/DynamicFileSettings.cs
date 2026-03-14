namespace DomuWave.Services.Settings;

/// <summary>
/// Configurazione del repository dinamico di file.
/// Aggiungere in appsettings.json sotto la chiave "DynamicFile".
/// </summary>
public class DynamicFileSettings
{
    /// <summary>
    /// Id del tipo di storage predefinito (0=Database, 1=FileSystem, 2=AzureBlob).
    /// Default: 0 (Database).
    /// </summary>
    public int DefaultStorageTypeId { get; set; } = 0;

    /// <summary>
    /// Cartella base per il provider FileSystem.
    /// Obbligatorio se DefaultStorageTypeId = 1.
    /// </summary>
    public string? FileSystemBasePath { get; set; }
}
