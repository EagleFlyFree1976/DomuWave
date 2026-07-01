namespace DomuWave.Services.Dto.TenantDisplay;

/// <summary>Payload per l'upload del logo del tenant (immagine in base64).</summary>
public class UploadTenantLogoDto
{
    public string FileName    { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    /// <summary>Contenuto immagine in base64 (senza prefisso data URI).</summary>
    public string Base64Data  { get; set; } = string.Empty;
}

/// <summary>Contenuto binario del logo restituito dall'endpoint GET /logo.</summary>
public class TenantLogoContentDto
{
    public byte[] Content     { get; set; } = System.Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
}
