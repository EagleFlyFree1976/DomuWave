using System.ComponentModel.DataAnnotations;

namespace DomuWave.Services.Dto.TenantDisplay;

/// <summary>
/// Payload per il caricamento del logo del tenant (immagine codificata in base64).
/// Pattern coerente con UploadDynamicFileDto, ma senza riferimenti a entità/permessi.
/// </summary>
public class UploadTenantLogoDto
{
    [Required] public string FileName    { get; set; } = string.Empty;
    [Required] public string ContentType { get; set; } = string.Empty;
    [Required] public string Base64Data  { get; set; } = string.Empty;
}

/// <summary>Contenuto binario del logo, per servirlo dall'endpoint GET.</summary>
public class TenantLogoContentDto
{
    public byte[] Content     { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
}
