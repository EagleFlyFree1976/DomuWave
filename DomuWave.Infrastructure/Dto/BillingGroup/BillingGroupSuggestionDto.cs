namespace DomuWave.Services.Dto.BillingGroup;

public class BillingGroupSuggestionDto
{
    public string SuggestedName  { get; set; }
    public string ContactEmail   { get; set; }
    public IList<BillingGroupUnitDto> Units { get; set; } = new List<BillingGroupUnitDto>();
}

public class SuggestBillingGroupsResultDto
{
    public IList<BillingGroupSuggestionDto> Suggestions  { get; set; } = new List<BillingGroupSuggestionDto>();
    public IList<BillingGroupUnitDto>       AmbiguousUnits { get; set; } = new List<BillingGroupUnitDto>();
}
