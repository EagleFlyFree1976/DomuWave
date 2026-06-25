using DomuWave.Services.Dto.Contabilita.Bilancio;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Contabilita.Bilancio;

public class GetContoEconomicoCommand : BaseCommand, IQuery<ContoEconomicoDto>
{
    public int FiscalYearId { get; set; }

    public GetContoEconomicoCommand() { }
    public GetContoEconomicoCommand(int currentUserId, int fiscalYearId) : base(currentUserId)
        => FiscalYearId = fiscalYearId;
}
