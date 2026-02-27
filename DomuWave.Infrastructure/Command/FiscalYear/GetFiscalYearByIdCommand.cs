using DomuWave.Services.Dto.Contabilita.FiscalYear;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FiscalYear;

public class GetFiscalYearByIdCommand : BaseCommand, IQuery<FiscalYearReadDto>
{
    public int FiscalYearId { get; set; }

    public GetFiscalYearByIdCommand() { }

    public GetFiscalYearByIdCommand(int currentUserId) : base(currentUserId) { }
    public GetFiscalYearByIdCommand(int currentUserId, int fiscalYearId) : base(currentUserId)
    {
        FiscalYearId = fiscalYearId;
    }
}
