using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.Consumption;

public class ConsumptionChargeReadDto : TraceEntityDTO<int>
{
    public int     ConsumptionTypeId   { get; set; }
    public string  ConsumptionTypeName { get; set; }
    public string  UnitOfMeasure       { get; set; }
    public int     FiscalYearId        { get; set; }
    public string  FiscalYearCode      { get; set; }
    public int     BudgetId            { get; set; }
    public string  BudgetCode          { get; set; }
    public int?    ExpenseId           { get; set; }
    public decimal TotalAmount         { get; set; }
    public int     StatusId            { get; set; }
    public string  StatusName          { get; set; }
    public string  Notes               { get; set; }
    public bool    HasWarnings         { get; set; }

    public IList<ConsumptionChargeItemReadDto> Items { get; set; } = [];
}

public class ConsumptionChargeItemReadDto
{
    public int     Id               { get; set; }
    public int     UnitId           { get; set; }
    public string  UnitName         { get; set; }
    public decimal Consumption      { get; set; }
    public decimal TotalConsumption { get; set; }
    public decimal Percentage       { get; set; }
    public decimal Amount           { get; set; }
    public bool    HasWarning       { get; set; }
}

public class CreateConsumptionChargeDto
{
    public int     ConsumptionTypeId { get; set; }
    public int     FiscalYearId      { get; set; }
    public int     BudgetId          { get; set; }
    public int?    ExpenseId         { get; set; }
    public decimal TotalAmount       { get; set; }
    public string? Notes             { get; set; }
}

public class UpdateConsumptionChargeDto
{
    public int?    BudgetId    { get; set; }
    public int?    ExpenseId   { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes       { get; set; }
}
