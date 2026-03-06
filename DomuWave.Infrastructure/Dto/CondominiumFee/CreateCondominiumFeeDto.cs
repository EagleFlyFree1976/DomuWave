namespace DomuWave.Services.Dto.CondominiumFee;

public class CreateCondominiumFeeDto
{
    public int      InstallmentId   { get; set; }
    public int      UnitId          { get; set; }
    public long     UserId          { get; set; }
    public decimal  AmountDue       { get; set; }
    public string?  Notes           { get; set; }
}
