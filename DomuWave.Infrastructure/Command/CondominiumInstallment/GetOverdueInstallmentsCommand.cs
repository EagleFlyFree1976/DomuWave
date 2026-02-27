using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumInstallment;

public class GetOverdueInstallmentsCommand : BaseCommand, IQuery<IList<Models.CondominiumInstallment>>
{
    public int CondominiumId { get; set; }

    public GetOverdueInstallmentsCommand() { }

    public GetOverdueInstallmentsCommand(int currentUserId) : base(currentUserId) { }
    public GetOverdueInstallmentsCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
