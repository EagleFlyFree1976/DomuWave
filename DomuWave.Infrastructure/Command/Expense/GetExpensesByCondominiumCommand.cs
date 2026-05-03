using DomuWave.Services.Dto.Expense;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Expense;

public class GetExpensesByCondominiumCommand : BaseCommand, IQuery<IList<ExpenseReadDto>>
{
    public int  CondominiumId { get; set; }
    public Guid TenantId      { get; set; }

    public GetExpensesByCondominiumCommand() { }

    public GetExpensesByCondominiumCommand(int currentUserId, int condominiumId, Guid tenantId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        TenantId      = tenantId;
    }
}
