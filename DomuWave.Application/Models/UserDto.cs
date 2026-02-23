using DomuWave.Services.Dto.Tenant;

namespace DomuWave.Application.Models
{
    public class UserDto : CPQ.Core.DTO.UserDto
    {
        public TenantReadDto? Tenant { get; set; }

        public IList<TenantReadDto> AvailableTenants { get; set; }
    }
}
