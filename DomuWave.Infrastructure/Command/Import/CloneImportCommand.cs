using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Import;

public class CloneImportCommand : BaseBookRelatedCommand, IQuery<Models.Import.Import>
{
    public long? TargetAccountId { get; set; }

    public long ImportId { get; set; }
}