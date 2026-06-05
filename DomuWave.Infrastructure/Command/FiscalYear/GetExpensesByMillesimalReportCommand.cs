using DomuWave.Services.Dto.Contabilita.FiscalYear;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FiscalYear;

public class GetExpensesByMillesimalReportCommand : BaseCommand, IQuery<ExpensesByMillesimalReportDto>
{
    public int FiscalYearId { get; set; }

    public GetExpensesByMillesimalReportCommand() { }

    public GetExpensesByMillesimalReportCommand(int currentUserId, int fiscalYearId) : base(currentUserId)
    {
        FiscalYearId = fiscalYearId;
    }
}
