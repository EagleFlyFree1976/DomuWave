using DomuWave.Services.Dto.UnitOwner;
using DomuWave.Services.Dto.UnitTenant;

namespace DomuWave.Services.Dto.RealEstateUnit;

public class RealEstateUnitPanoramaDto : RealEstateUnitReadDto
{
    public IList<UnitOwnerReadDto>  Owners  { get; set; } = new List<UnitOwnerReadDto>();
    public IList<UnitTenantReadDto> Tenants { get; set; } = new List<UnitTenantReadDto>();
}
