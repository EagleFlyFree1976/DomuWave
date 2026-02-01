using DomuWave.Services.Models.Dto.Import;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Import;

/// <summary>
///    Una vol
/// </summary>
public class ValidateImportCommand : BaseBookRelatedCommand, IQuery<ImportDto>
{
    public long ImportId { get; set; }
}