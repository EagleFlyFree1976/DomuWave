using DomuWave.Services.Models;

namespace DomuWave.Services.Dto.Expense;

public class CreateExpenseDto
{
    public int      CondominiumId     { get; set; }
    public int?     FiscalYearId      { get; set; }
    public int      AccountId         { get; set; }
    public int?     SupplierId        { get; set; }
    public int      MillesimalTableId { get; set; }
    public string   Name              { get; set; } = string.Empty;
    public string?  DocumentNumber    { get; set; }
    public DateTime DocumentDate      { get; set; }
    public DateTime RegistrationDate  { get; set; }
    public decimal  TaxableAmount          { get; set; }
    public decimal  TaxableAmountVatExempt { get; set; }
    public decimal  GrossAmount            { get; set; }
    public decimal  VatAmount              { get; set; }
    public decimal  NetAmount              { get; set; }
    public decimal  PensionFund            { get; set; }
    public decimal  WithholdingTax         { get; set; }
    public decimal  StampDuty              { get; set; }
    public int      ExpenseTypeId        { get; set; }
    public int      PaymentStatusId      { get; set; } = 1;
    public string?  PaymentMethod        { get; set; }
    public string?  Description          { get; set; }
    public int      ChargeabilityTypeId  { get; set; } = ChargeabilityType.Owner;
}
