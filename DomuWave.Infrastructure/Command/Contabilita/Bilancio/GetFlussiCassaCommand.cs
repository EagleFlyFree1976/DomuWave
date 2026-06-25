using DomuWave.Services.Dto.Contabilita.Bilancio;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Contabilita.Bilancio;

public class GetFlussiCassaCommand : BaseCommand, IQuery<FlussiCassaDto>
{
    public int FiscalYearId { get; set; }

    public GetFlussiCassaCommand() { }
    public GetFlussiCassaCommand(int currentUserId, int fiscalYearId) : base(currentUserId)
        => FiscalYearId = fiscalYearId;
}
