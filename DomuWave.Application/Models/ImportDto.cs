using DomuWave.Services.Models.Dto.Import;

namespace DomuWave.Application.Models;

public class ImportDto
{
    public long TargetAccountId { get; set; }

    public bool HasHeader { get; set; }

    public string Name { get; set; }

}



public class UpdateImportDto
{
    public long TargetAccountId { get; set; }

    public string Name { get; set; }

    public UpdateImportConfigurationDto Configuration { get; set; }
}