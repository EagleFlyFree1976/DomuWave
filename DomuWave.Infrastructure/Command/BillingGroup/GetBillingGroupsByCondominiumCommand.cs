using DomuWave.Services.Dto.BillingGroup;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.BillingGroup;

public class GetBillingGroupsByCondominiumCommand : BaseCommand, IQuery<IList<BillingGroupReadDto>>
{
    public int CondominiumId { get; set; }

    public GetBillingGroupsByCondominiumCommand() { }
    public GetBillingGroupsByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
        => CondominiumId = condominiumId;
}
