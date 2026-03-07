using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FiscalYear;

public class OpenFiscalYearFromDraftCommand : BaseCommand, IQuery<bool>
{
    public int FiscalYearId { get; set; }

    public OpenFiscalYearFromDraftCommand() { }

    public OpenFiscalYearFromDraftCommand(int currentUserId, int fiscalYearId) : base(currentUserId)
    {
        FiscalYearId = fiscalYearId;
    }
}
