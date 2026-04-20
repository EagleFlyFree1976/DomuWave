using DomuWave.Services.Dto.BillingGroup;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.BillingGroup;

public class ApplySuggestedBillingGroupsCommand : BaseCommand, IQuery<IList<BillingGroupReadDto>>
{
    public int CondominiumId              { get; set; }
    public IList<BillingGroupSuggestionDto> Suggestions { get; set; } = new List<BillingGroupSuggestionDto>();

    public ApplySuggestedBillingGroupsCommand() { }
    public ApplySuggestedBillingGroupsCommand(int currentUserId, int condominiumId, IList<BillingGroupSuggestionDto> suggestions)
        : base(currentUserId)
    {
        CondominiumId = condominiumId;
        Suggestions   = suggestions;
    }
}
