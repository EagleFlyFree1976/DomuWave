using DomuWave.Services.Dto.Expense;
using DomuWave.Services.Helper;
using DomuWave.Services.Models;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Expense;

public class GetPagedExpensesCommand : BasePagedCommand, IQuery<PagedResult<ExpenseReadDto>>
{
    public int  CondominiumId   { get; set; }
    public Guid TenantId        { get; set; }

    // Filtri opzionali
    public int?      ExpenseTypeId   { get; set; }
    public int?      PaymentStatusId { get; set; }
    public int?      SupplierId      { get; set; }
    public int?      FiscalYearId    { get; set; }
    public DateTime? DateFrom        { get; set; }
    public DateTime? DateTo          { get; set; }
    public string?   Search          { get; set; }
    public ChartOfAccountsType? AccountType { get; set; }

    public GetPagedExpensesCommand() { }
    public GetPagedExpensesCommand(int currentUserId, int condominiumId, Guid tenantId)
        : base(currentUserId)
    {
        CondominiumId = condominiumId;
        TenantId      = tenantId;
    }
}
