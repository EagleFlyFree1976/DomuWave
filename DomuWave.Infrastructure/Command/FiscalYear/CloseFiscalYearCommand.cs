using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FiscalYear;

public class CloseFiscalYearCommand : BaseCommand, IQuery<bool>
{
    public int FiscalYearId { get; set; }
    public string Notes { get; set; }

    public CloseFiscalYearCommand() { }

    public CloseFiscalYearCommand(int currentUserId) : base(currentUserId) { }
    public CloseFiscalYearCommand(int currentUserId, int fiscalYearId, string notes) : base(currentUserId)
    {
        FiscalYearId = fiscalYearId;
        Notes = notes;
    }
}
