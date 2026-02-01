namespace DomuWave.Services.Models.Import;

public enum ImportStatus 
{
    Pending,
    Duplicate,
    Excluded,
    ToBeImported,
    Processed,
    Error
}