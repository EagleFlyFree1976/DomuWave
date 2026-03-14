using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.DynamicFile;

public class DynamicFileReadDto : TraceEntityDTO<int>
{
    public string EntityFullName    { get; set; }
    public string EntityName        { get; set; }
    public int    EntityId          { get; set; }

    public string FileName          { get; set; }
    public string FileExtension     { get; set; }
    public string ContentType       { get; set; }
    public long   FileSize          { get; set; }
    public string? Description      { get; set; }
    public string? Tags             { get; set; }

    public int    StorageTypeId     { get; set; }
    public string StorageTypeName   { get; set; }

    /// <summary>Valorizzato solo su richiesta esplicita di download (GET by id con contenuto)</summary>
    public string? Base64Data       { get; set; }
}
