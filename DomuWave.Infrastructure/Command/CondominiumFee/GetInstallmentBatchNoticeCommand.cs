using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumFee;

/// <summary>
/// Genera il PDF batch dell'avviso di pagamento per una singola rata:
/// produce un documento multi-pagina con una pagina per ogni unità della rata.
/// </summary>
public class GetInstallmentBatchNoticeCommand : BaseCommand, IQuery<byte[]>
{
    public int InstallmentId { get; set; }

    public GetInstallmentBatchNoticeCommand() { }

    public GetInstallmentBatchNoticeCommand(int currentUserId, int installmentId)
        : base(currentUserId)
    {
        InstallmentId = installmentId;
    }
}
