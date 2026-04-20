using DomuWave.Services.Dto.BillingGroup;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.BillingGroup;

public class CreateBillingGroupCommand : BaseCommand, IQuery<BillingGroupReadDto>
{
    public CreateBillingGroupDto Dto { get; set; }

    public CreateBillingGroupCommand() { }
    public CreateBillingGroupCommand(int currentUserId, CreateBillingGroupDto dto) : base(currentUserId)
        => Dto = dto;
}
