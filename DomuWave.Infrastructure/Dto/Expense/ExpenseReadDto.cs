using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.Expense;

public class ExpenseReadDto : TraceEntityDTO<long>
{
    public int      CondominiumId      { get; set; }
    public int?     SupplierId         { get; set; }
    public string?  SupplierName       { get; set; }
    public int      AccountId          { get; set; }
    public string?  AccountCode        { get; set; }
    public string?  AccountName        { get; set; }
    public int      MillesimalTableId  { get; set; }
    public string?  MillesimalTableCode { get; set; }
    public string   Name               { get; set; } = string.Empty;
    public string?  DocumentNumber     { get; set; }
    public DateTime DocumentDate       { get; set; }
    public DateTime RegistrationDate   { get; set; }
    public decimal  GrossAmount        { get; set; }
    public decimal  VatAmount          { get; set; }
    public decimal  NetAmount          { get; set; }
    public string   ExpenseType        { get; set; } = string.Empty;
    public string   PaymentStatus      { get; set; } = string.Empty;
    public DateTime? PaymentDate       { get; set; }
    public string?  PaymentMethod      { get; set; }
    public string?  Description        { get; set; }
}
