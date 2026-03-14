using CPQ.Core;

namespace DomuWave.Services.Models;

/// <summary>
/// Metadati di un file nel repository dinamico.
/// Collegato a qualsiasi entita' tramite EntityFullName + EntityName + EntityId.
/// </summary>
public class DynamicFile : TraceEntity<int>
{
    /// <summary>Nome completo del tipo entita' (es. "DomuWave.Services.Models.Expense")</summary>
    public virtual string EntityFullName { get; set; }

    /// <summary>Nome semplice del tipo entita' (es. "Expense")</summary>
    public virtual string EntityName { get; set; }

    /// <summary>Id dell'entita' a cui il file e' collegato</summary>
    public virtual int EntityId { get; set; }

    /// <summary>Nome originale del file</summary>
    public virtual string FileName { get; set; }

    /// <summary>Estensione del file (es. ".pdf")</summary>
    public virtual string FileExtension { get; set; }

    /// <summary>MIME type (es. "application/pdf", "image/png")</summary>
    public virtual string ContentType { get; set; }

    /// <summary>Dimensione in byte</summary>
    public virtual long FileSize { get; set; }

    /// <summary>Descrizione opzionale del file</summary>
    public virtual string Description { get; set; }

    /// <summary>Tag separati da virgola</summary>
    public virtual string Tags { get; set; }

    /// <summary>Tipo di storage usato per il contenuto</summary>
    public virtual DynamicFileStorageType StorageType { get; set; }

    /// <summary>Riferimento al contenuto esterno (path filesystem o URL blob); null per storage Database</summary>
    public virtual string StorageReference { get; set; }

    public virtual Tenant Tenant { get; set; }

    public override int GetHashCode() => Id.GetHashCode();
}

/// <summary>
/// Contenuto base64 del file — presente solo quando StorageType = Database.
/// Separato dai metadati per non appesantire le query di lista.
/// </summary>
public class DynamicFileContent : TraceEntity<int>
{
    public virtual DynamicFile File { get; set; }

    /// <summary>Contenuto del file codificato in base64</summary>
    public virtual string Base64Data { get; set; }

    public override int GetHashCode() => Id.GetHashCode();
}

/// <summary>Lookup: tipo di storage (0=Database, 1=FileSystem, 2=AzureBlob)</summary>
public class DynamicFileStorageType
{
    public virtual int Id { get; set; }
    public virtual string Name { get; set; }

    public override int GetHashCode() => Id.GetHashCode();
}
