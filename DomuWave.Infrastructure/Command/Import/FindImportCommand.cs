using DomuWave.Services.Models.Dto.Import;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Import;

public class FindImportCommand : BaseBookRelatedCommand, IQuery<IList<ImportMinDto>>
{
    public long ImportId { get; set; }
}