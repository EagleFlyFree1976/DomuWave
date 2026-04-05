using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FiscalYear;

public class DeleteFiscalYearCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }

    public DeleteFiscalYearCommand() { }

    public DeleteFiscalYearCommand(int currentUserId, int id) : base(currentUserId)
    {
        Id = id;
    }
}
