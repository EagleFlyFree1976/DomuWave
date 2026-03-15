using DomuWave.Services.Dto.CondominiumInstallment;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumInstallment;

public class GetInstallmentsByBudgetCommand : BaseCommand, IQuery<IList<CondominiumInstallmentReadDto>>
{
    public int BudgetId { get; set; }

    public GetInstallmentsByBudgetCommand() { }
    public GetInstallmentsByBudgetCommand(int currentUserId, int budgetId) : base(currentUserId)
        => BudgetId = budgetId;
}
