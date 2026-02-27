using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumInstallment;

public class GetOpenInstallmentsCommand : BaseCommand, IQuery<IList<Models.CondominiumInstallment>>
{
    public int CondominiumId { get; set; }

    public GetOpenInstallmentsCommand() { }

    public GetOpenInstallmentsCommand(int currentUserId) : base(currentUserId) { }
    public GetOpenInstallmentsCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
