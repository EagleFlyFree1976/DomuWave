using DomuWave.Services.Dto.RealEstateUnit;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.RealEstateUnit;

public class GetRealEstateUnitByIdCommand : BaseTenantRelatedCommand, IQuery<RealEstateUnitReadDto>
{
    public int UnitId { get; set; }

    public GetRealEstateUnitByIdCommand() { }

    public GetRealEstateUnitByIdCommand(int currentUserId) : base(currentUserId) { }
    public GetRealEstateUnitByIdCommand(int currentUserId, Guid tenantId, int unitId) : base(currentUserId, tenantId)
    {
        UnitId = unitId;
    }
}
