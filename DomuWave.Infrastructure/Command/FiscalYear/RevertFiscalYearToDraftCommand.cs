using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FiscalYear;

public class RevertFiscalYearToDraftCommand : BaseCommand, IQuery<bool>
{
    public int FiscalYearId { get; set; }

    public RevertFiscalYearToDraftCommand() { }

    public RevertFiscalYearToDraftCommand(int currentUserId, int fiscalYearId) : base(currentUserId)
    {
        FiscalYearId = fiscalYearId;
    }
}
