using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumInstallment;

public class GenerateInstallmentsCommand : BaseCommand, IQuery<bool>
{
    public int CondominiumId { get; set; }
    public int FiscalYearId  { get; set; }
    public int BudgetId      { get; set; }

    public GenerateInstallmentsCommand() { }

    public GenerateInstallmentsCommand(int currentUserId) : base(currentUserId) { }
    public GenerateInstallmentsCommand(int currentUserId, int condominiumId, int fiscalYearId, int budgetId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        FiscalYearId  = fiscalYearId;
        BudgetId      = budgetId;
    }
}
