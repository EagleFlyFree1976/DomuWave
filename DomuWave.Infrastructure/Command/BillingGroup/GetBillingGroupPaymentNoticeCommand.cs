using SimpleMediator.Queries;

namespace DomuWave.Services.Command.BillingGroup;

/// <summary>
/// Genera il PDF dell'avviso di pagamento per un billing group:
/// include tutte le rate dell'esercizio per ogni unità del gruppo.
/// </summary>
public class GetBillingGroupPaymentNoticeCommand : BaseCommand, IQuery<byte[]>
{
    public int BillingGroupId { get; set; }
    public int FiscalYearId   { get; set; }

    public GetBillingGroupPaymentNoticeCommand() { }

    public GetBillingGroupPaymentNoticeCommand(int currentUserId, int billingGroupId, int fiscalYearId)
        : base(currentUserId)
    {
        BillingGroupId = billingGroupId;
        FiscalYearId   = fiscalYearId;
    }
}

/// <summary>
/// Genera il PDF dell'avviso di pagamento per un billing group limitato a una singola rata.
/// </summary>
public class GetBillingGroupInstallmentNoticeCommand : BaseCommand, IQuery<byte[]>
{
    public int BillingGroupId  { get; set; }
    public int InstallmentId   { get; set; }

    public GetBillingGroupInstallmentNoticeCommand() { }

    public GetBillingGroupInstallmentNoticeCommand(int currentUserId, int billingGroupId, int installmentId)
        : base(currentUserId)
    {
        BillingGroupId = billingGroupId;
        InstallmentId  = installmentId;
    }
}
