using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FiscalYear;

public class DeleteFiscalYearCommand : BaseTenantRelatedCommand, IQuery<bool>
{
    public int Id { get; set; }

    public DeleteFiscalYearCommand() { }

    public DeleteFiscalYearCommand(int currentUserId, Guid tenantId, int id) : base(currentUserId, tenantId)
    {
        Id = id;
    }
}
