using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FiscalYear;

public class StartClosingFiscalYearCommand : BaseCommand, IQuery<bool>
{
    public int FiscalYearId { get; set; }
    public string Notes { get; set; }

    public StartClosingFiscalYearCommand() { }

    public StartClosingFiscalYearCommand(int currentUserId) : base(currentUserId) { }
    public StartClosingFiscalYearCommand(int currentUserId, int fiscalYearId, string notes) : base(currentUserId)
    {
        FiscalYearId = fiscalYearId;
        Notes = notes;
    }
}
