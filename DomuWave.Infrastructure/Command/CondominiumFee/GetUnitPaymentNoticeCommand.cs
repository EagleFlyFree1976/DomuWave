using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumFee;

/// <summary>
/// Genera il PDF dell'avviso di pagamento per una singola unità:
/// include tutte le rate dell'esercizio fiscale specificato.
/// </summary>
public class GetUnitPaymentNoticeCommand : BaseCommand, IQuery<byte[]>
{
    public int UnitId       { get; set; }
    public int FiscalYearId { get; set; }

    public GetUnitPaymentNoticeCommand() { }

    public GetUnitPaymentNoticeCommand(int currentUserId, int unitId, int fiscalYearId)
        : base(currentUserId)
    {
        UnitId       = unitId;
        FiscalYearId = fiscalYearId;
    }
}

/// <summary>
/// Genera il PDF dell'avviso di pagamento per una singola unità limitato a una singola rata.
/// </summary>
public class GetUnitInstallmentPaymentNoticeCommand : BaseCommand, IQuery<byte[]>
{
    public int UnitId         { get; set; }
    public int InstallmentId  { get; set; }

    public GetUnitInstallmentPaymentNoticeCommand() { }

    public GetUnitInstallmentPaymentNoticeCommand(int currentUserId, int unitId, int installmentId)
        : base(currentUserId)
    {
        UnitId        = unitId;
        InstallmentId = installmentId;
    }
}
