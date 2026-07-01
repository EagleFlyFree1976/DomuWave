using DomuWave.Services.Dto.Condominium;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Condominium;

public class UpdateCondominiumInitialBalanceCommand : BaseTenantRelatedCommand, IQuery<CondominiumReadDto>
{
    public int CondominiumId { get; set; }
    public decimal InitialBalance { get; set; }

    public UpdateCondominiumInitialBalanceCommand() { }

    public UpdateCondominiumInitialBalanceCommand(int currentUserId) : base(currentUserId) { }

    public UpdateCondominiumInitialBalanceCommand(int currentUserId, Guid tenantId, int condominiumId, decimal initialBalance)
        : base(currentUserId, tenantId)
    {
        CondominiumId  = condominiumId;
        InitialBalance = initialBalance;
    }
}
