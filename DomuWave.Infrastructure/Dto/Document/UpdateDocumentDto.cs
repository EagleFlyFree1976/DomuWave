namespace DomuWave.Services.Dto.Document;

public class UpdateDocumentDto
{
    public string?   Title             { get; set; }
    public string?   Category          { get; set; }
    public DateTime? DocumentDate      { get; set; }
    public bool?     IsVisibleToOwners { get; set; }
    public bool?     IsArchived        { get; set; }
    public string?   Tags              { get; set; }
    public string?   Description       { get; set; }
}
