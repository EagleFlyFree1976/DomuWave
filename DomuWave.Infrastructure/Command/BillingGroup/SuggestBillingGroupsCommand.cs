using DomuWave.Services.Dto.BillingGroup;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.BillingGroup;

public class SuggestBillingGroupsCommand : BaseCommand, IQuery<SuggestBillingGroupsResultDto>
{
    public int CondominiumId { get; set; }

    public SuggestBillingGroupsCommand() { }
    public SuggestBillingGroupsCommand(int currentUserId, int condominiumId) : base(currentUserId)
        => CondominiumId = condominiumId;
}
