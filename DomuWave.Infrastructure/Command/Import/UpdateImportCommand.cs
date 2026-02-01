using DomuWave.Services.Models.Dto.Import;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Import;

public class UpdateImportCommand : BaseBookRelatedCommand, IQuery<Models.Import.Import>
{
    public long ImportId { get; set; }


    public long TargetAccountId { get; set; } 


    public string Name { get; set; }
    public UpdateImportConfigurationDto ImportConfigurationDto { get; set; }



}