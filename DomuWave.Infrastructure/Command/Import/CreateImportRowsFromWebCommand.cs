using DomuWave.Services.Models.Dto.Import;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Import;

public class CreateImportRowsFromWebCommand : BaseBookRelatedCommand, IQuery<bool>
{
    public long ImportId { get; set; }
    public IList<ImportRowDto> Rows { get; set; }
}