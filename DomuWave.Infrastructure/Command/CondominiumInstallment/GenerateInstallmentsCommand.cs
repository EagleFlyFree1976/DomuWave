using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumInstallment;

public class GenerateInstallmentsCommand : BaseCommand, IQuery<bool>
{
    public int CondominiumId { get; set; }
    public int Year { get; set; }
    public int BudgetId { get; set; }

    public GenerateInstallmentsCommand() { }

    public GenerateInstallmentsCommand(int currentUserId) : base(currentUserId) { }
    public GenerateInstallmentsCommand(int currentUserId, int condominiumId, int year, int budgetId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        Year = year;
        BudgetId = budgetId;
    }
}
