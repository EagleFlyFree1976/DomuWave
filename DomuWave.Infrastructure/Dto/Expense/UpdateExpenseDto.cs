namespace DomuWave.Services.Dto.Expense;

public class UpdateExpenseDto
{
    public int      AccountId         { get; set; }
    public int?     SupplierId        { get; set; }
    public int      MillesimalTableId { get; set; }
    public string   Name              { get; set; } = string.Empty;
    public string?  DocumentNumber    { get; set; }
    public DateTime DocumentDate      { get; set; }
    public DateTime RegistrationDate  { get; set; }
    public decimal  GrossAmount       { get; set; }
    public decimal  VatAmount         { get; set; }
    public decimal  NetAmount         { get; set; }
    public int      ExpenseTypeId        { get; set; }
    public int      PaymentStatusId      { get; set; } = 1;
    public string?  PaymentMethod        { get; set; }
    public string?  Description          { get; set; }
    public int      ChargeabilityTypeId  { get; set; } = 0;
}
