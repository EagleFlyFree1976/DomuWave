using DomuWave.Services.Dto.RealEstateUnit;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.RealEstateUnit;

public class UpdateRealEstateUnitCommand : BaseTenantRelatedCommand, IQuery<RealEstateUnitReadDto>
{
    public int UnitId { get; set; }
    public UpdateRealEstateUnitDto Dto { get; set; }

    public UpdateRealEstateUnitCommand() { }

    public UpdateRealEstateUnitCommand(int currentUserId) : base(currentUserId) { }
    public UpdateRealEstateUnitCommand(int currentUserId, Guid tenantId, int unitId, UpdateRealEstateUnitDto dto) : base(currentUserId, tenantId)
    {
        UnitId = unitId;
        Dto    = dto;
    }
}
