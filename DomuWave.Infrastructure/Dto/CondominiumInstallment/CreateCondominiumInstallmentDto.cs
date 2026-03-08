using DomuWave.Services.Models;

namespace DomuWave.Services.Dto.CondominiumInstallment;

public class CreateCondominiumInstallmentDto
{
    public int      CondominiumId     { get; set; }
    public int?     BudgetId          { get; set; }
    public int?     FiscalYearId      { get; set; }
    public int      InstallmentNumber { get; set; }
    public DateTime DueDate           { get; set; }
    public decimal  TotalAmount       { get; set; }
    public int      StatusId          { get; set; } = CondominiumInstallmentStatus.Open;
    public string?  Notes             { get; set; }
}
