using DomuWave.Services.Dto.CondominiumFee;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumFee;

public class GetFeesByUserCommand : BaseCommand, IQuery<IList<CondominiumFeeReadDto>>
{
    public long UserId { get; set; }
    public Guid TenantId { get; set; }

    public GetFeesByUserCommand() { }

    public GetFeesByUserCommand(int currentUserId) : base(currentUserId) { }
    public GetFeesByUserCommand(int currentUserId, long userId, Guid tenantId) : base(currentUserId)
    {
        UserId   = userId;
        TenantId = tenantId;
    }
}
