using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.Expense;

public class ExpenseReadDto : TraceEntityDTO<long>
{
    public Guid     TenantId           { get; set; }
    public int      CondominiumId      { get; set; }
    public int?     FiscalYearId       { get; set; }
    public string?  FiscalYearCode     { get; set; }
    public int?     SupplierId         { get; set; }
    public string?  SupplierName       { get; set; }
    public int      AccountId          { get; set; }
    public string   AccountName        { get; set; } = string.Empty;
    public int      MillesimalTableId  { get; set; }
    public string   MillesimalTableName { get; set; } = string.Empty;
    public string   Name               { get; set; } = string.Empty;
    public string?  DocumentNumber     { get; set; }
    public DateTime DocumentDate       { get; set; }
    public DateTime RegistrationDate   { get; set; }
    public decimal  GrossAmount        { get; set; }
    public decimal  VatAmount          { get; set; }
    public decimal  NetAmount          { get; set; }
    public int      ExpenseTypeId      { get; set; }
    public string   ExpenseTypeName    { get; set; } = string.Empty;
    public int      PaymentStatusId    { get; set; }
    public string   PaymentStatusName  { get; set; } = string.Empty;
    public DateTime? PaymentDate       { get; set; }
    public string?  PaymentMethod      { get; set; }
    public string?  Description        { get; set; }
    public int      ChargeabilityTypeId   { get; set; }
    public string   ChargeabilityTypeName { get; set; } = string.Empty;
}
