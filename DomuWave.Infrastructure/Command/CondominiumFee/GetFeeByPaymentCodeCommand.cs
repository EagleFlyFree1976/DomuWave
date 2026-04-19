using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumFee;

/// <summary>
/// Cerca una o più fee per codici di riconciliazione (la stringa può contenere
/// più codici separati da "/" o spazi, come appare nella causale del bonifico).
/// </summary>
public class GetFeeByPaymentCodeCommand : BaseCommand, IQuery<IList<FeeByPaymentCodeResultDto>>
{
    public string PaymentCode { get; set; } = string.Empty;

    public GetFeeByPaymentCodeCommand() { }

    public GetFeeByPaymentCodeCommand(int currentUserId, string paymentCode)
        : base(currentUserId)
    {
        PaymentCode = paymentCode;
    }
}

public class FeeByPaymentCodeResultDto
{
    public long     FeeId              { get; set; }
    public string   PaymentCode        { get; set; } = string.Empty;
    public int      UnitId             { get; set; }
    public string   UnitInternalNumber { get; set; } = string.Empty;
    public string?  UnitDisplayName    { get; set; }
    public int      InstallmentId      { get; set; }
    public int      InstallmentNumber  { get; set; }
    public DateTime DueDate            { get; set; }
    public string?  FiscalYearCode     { get; set; }
    public string?  CondominiumName    { get; set; }
    public decimal  AmountDue          { get; set; }
    public decimal  AmountPaid         { get; set; }
    public decimal  Balance            { get; set; }
    public string   PaymentStatus      { get; set; } = string.Empty;
    public string?  OwnerFullName      { get; set; }
}
