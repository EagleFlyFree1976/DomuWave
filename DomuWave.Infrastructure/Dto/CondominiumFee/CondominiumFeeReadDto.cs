using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.CondominiumFee;

public class CondominiumFeeReadDto : TraceEntityDTO<long>
{
    public int      InstallmentId   { get; set; }
    public int      UnitId          { get; set; }
    public string?  UnitName        { get; set; }
    public long     UserId          { get; set; }
    public decimal  AmountDue       { get; set; }
    public decimal  AmountPaid      { get; set; }
    public decimal  Balance         { get; set; }
    public string   PaymentStatus   { get; set; } = string.Empty;
    public DateTime? PaymentDate    { get; set; }
    public string?  PaymentMethod   { get; set; }
    public string?  Notes           { get; set; }
}
