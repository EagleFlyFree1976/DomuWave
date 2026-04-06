using DomuWave.Services.Dto.Contabilita.FiscalYear;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FiscalYear;

public class GetConguaglioCommand : BaseCommand, IQuery<ConguaglioReadDto>
{
    public int FiscalYearId { get; set; }

    public GetConguaglioCommand() { }

    public GetConguaglioCommand(int currentUserId, int fiscalYearId) : base(currentUserId)
    {
        FiscalYearId = fiscalYearId;
    }
}
