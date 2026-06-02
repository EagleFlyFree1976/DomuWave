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
    public int?     MillesimalTableId  { get; set; }
    public string   MillesimalTableName { get; set; } = string.Empty;
    public int?     UnitId             { get; set; }
    public string?  UnitName           { get; set; }
    public string   Name               { get; set; } = string.Empty;
    public string?  DocumentNumber     { get; set; }
    public DateTime? DocumentDate      { get; set; }
    public DateTime? RegistrationDate  { get; set; }
    public decimal  TaxableAmount          { get; set; }
    public decimal  TaxableAmountVatExempt { get; set; }
    public decimal  GrossAmount            { get; set; }
    public decimal  VatAmount              { get; set; }
    public decimal  NetAmount              { get; set; }
    public decimal  PensionFund            { get; set; }
    public decimal  WithholdingTax         { get; set; }
    public decimal  StampDuty              { get; set; }
    public int      ExpenseTypeId      { get; set; }
    public string   ExpenseTypeName    { get; set; } = string.Empty;
    public int      PaymentStatusId    { get; set; }
    public string   PaymentStatusName  { get; set; } = string.Empty;
    public DateTime? PaymentDate         { get; set; }
    public int?     PaymentMethodId     { get; set; }
    public string?  PaymentMethodName   { get; set; }
    public string?  Description        { get; set; }
    public int      ChargeabilityTypeId   { get; set; }
    public string   ChargeabilityTypeName { get; set; } = string.Empty;
    public bool     Send770               { get; set; }
}
