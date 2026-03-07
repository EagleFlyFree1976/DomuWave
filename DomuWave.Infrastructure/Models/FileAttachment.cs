using CPQ.Core;
using RabbitMQ.Client.Events;

namespace DomuWave.Services.Models;

/// <summary>
/// Metadati del file allegato, collegato genericamente a qualsiasi entita' tramite EntityKey + EntityId.
/// </summary>
public class FileAttachment : TraceEntity<int>
{
    /// <summary>Nome logico del tipo di entita' (es. "Expense", "Maintenance", "Supplier")</summary>
    public virtual string EntityKey { get; set; }

    /// <summary>Id dell'entita' a cui il file e' collegato</summary>
    public virtual int EntityId { get; set; }

    /// <summary>Nome originale del file</summary>
    public virtual string FileName { get; set; }

    /// <summary>MIME type (es. "application/pdf", "image/png")</summary>
    public virtual string ContentType { get; set; }

    /// <summary>Dimensione in byte</summary>
    public virtual long FileSize { get; set; }

    public virtual Tenant Tenant { get; set; }

    /// <summary>Contenuto binario del file (tabella separata)</summary>
    public virtual FileAttachmentContent Content { get; set; }
    public override int GetHashCode()
    {
        return this.Id.GetHashCode();
    }
}

/// <summary>
/// Contenuto base64 del file, separato dai metadati per non appesantire le query di lista.
/// </summary>
public class FileAttachmentContent : TraceEntity<int>
{
    /// <summary>Contenuto del file codificato in base64</summary>
    public virtual string Base64Data { get; set; }

    public virtual FileAttachment Attachment { get; set; }
    public override int GetHashCode()
    {
        return this.Id.GetHashCode();
    }
}
