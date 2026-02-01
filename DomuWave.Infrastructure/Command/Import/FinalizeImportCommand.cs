using DomuWave.Services.Models.Dto.Import;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Import;

public class FinalizeImportCommand : BaseBookRelatedCommand, IQuery<ImportDto>
{
    public long ImportId { get; set; }
}