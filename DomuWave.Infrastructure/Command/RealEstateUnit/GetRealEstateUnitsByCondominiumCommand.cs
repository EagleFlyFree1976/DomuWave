using DomuWave.Services.Dto.RealEstateUnit;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.RealEstateUnit;

public class GetRealEstateUnitsByCondominiumCommand : BaseCommand, IQuery<IList<RealEstateUnitReadDto>>
{
    public int  CondominiumId { get; set; }
    public Guid TenantId      { get; set; }

    public GetRealEstateUnitsByCondominiumCommand() { }

    public GetRealEstateUnitsByCondominiumCommand(int currentUserId, int condominiumId, Guid tenantId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        TenantId      = tenantId;
    }
}
