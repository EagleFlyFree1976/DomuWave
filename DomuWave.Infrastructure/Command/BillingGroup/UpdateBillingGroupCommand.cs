using DomuWave.Services.Dto.BillingGroup;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.BillingGroup;

public class UpdateBillingGroupCommand : BaseCommand, IQuery<BillingGroupReadDto>
{
    public int                  Id  { get; set; }
    public UpdateBillingGroupDto Dto { get; set; }

    public UpdateBillingGroupCommand() { }
    public UpdateBillingGroupCommand(int currentUserId, int id, UpdateBillingGroupDto dto) : base(currentUserId)
    {
        Id  = id;
        Dto = dto;
    }
}
