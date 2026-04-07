using DomuWave.Services.Dto.UnitOpeningBalance;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitOpeningBalance;

public class GetUnitOpeningBalancesByFiscalYearCommand : BaseCommand, IQuery<IList<UnitOpeningBalanceReadDto>>
{
    public int FiscalYearId { get; set; }

    public GetUnitOpeningBalancesByFiscalYearCommand() { }
    public GetUnitOpeningBalancesByFiscalYearCommand(int currentUserId, int fiscalYearId) : base(currentUserId)
    {
        FiscalYearId = fiscalYearId;
    }
}
