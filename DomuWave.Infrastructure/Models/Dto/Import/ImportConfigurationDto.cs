namespace DomuWave.Services.Models.Dto.Import;

public class ImportConfigurationDto
{

    public virtual string FileName { get; set; }
    public virtual bool HasHeader { get; set; }
    public virtual string Delimiter { get; set; }

    public virtual string CultureInfo { get; set; } = string.Empty;
    public List<ImportField> Fields { get; set; } = new List<ImportField>();

    

}

public class UpdateImportConfigurationDto
{
    public virtual string CultureInfo { get; set; } = string.Empty;
    public List<ImportField> Fields { get; set; } = new List<ImportField>();
    

}