using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FiscalYear;

public class LockFiscalYearCommand : BaseCommand, IQuery<bool>
{
    public int FiscalYearId { get; set; }
    public string Notes { get; set; }

    public LockFiscalYearCommand() { }

    public LockFiscalYearCommand(int currentUserId) : base(currentUserId) { }
    public LockFiscalYearCommand(int currentUserId, int fiscalYearId, string notes) : base(currentUserId)
    {
        FiscalYearId = fiscalYearId;
        Notes = notes;
    }
}
