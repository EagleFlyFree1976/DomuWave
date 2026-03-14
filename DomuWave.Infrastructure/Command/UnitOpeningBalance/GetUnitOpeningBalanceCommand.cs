using DomuWave.Services.Dto.UnitOpeningBalance;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitOpeningBalance;

public class GetUnitOpeningBalanceCommand : BaseCommand, IQuery<UnitOpeningBalanceReadDto>
{
    public int UnitId       { get; set; }
    public int FiscalYearId { get; set; }

    public GetUnitOpeningBalanceCommand() { }
    public GetUnitOpeningBalanceCommand(int currentUserId, int unitId, int fiscalYearId) : base(currentUserId)
    {
        UnitId       = unitId;
        FiscalYearId = fiscalYearId;
    }
}
