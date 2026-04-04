using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.CondominiumInstallment;

public class CondominiumInstallmentReadDto : TraceEntityDTO<int>
{
    public Guid     TenantId          { get; set; }
    public int      CondominiumId     { get; set; }
    public int?     BudgetId          { get; set; }
    public int?     FiscalYearId      { get; set; }
    public string?  FiscalYearCode    { get; set; }
    public string?  FiscalYearName    { get; set; }
    public int      InstallmentNumber { get; set; }
    public DateTime DueDate           { get; set; }
    public decimal  TotalAmount       { get; set; }
    public int      StatusId          { get; set; }
    public string   StatusName        { get; set; } = string.Empty;
    public string?  Notes             { get; set; }
}
