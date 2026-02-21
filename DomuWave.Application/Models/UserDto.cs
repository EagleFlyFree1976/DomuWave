namespace DomuWave.Application.Models
{
    public class UserDto : CPQ.Core.DTO.UserDto
    {
        public long? TenantId { get; set; }
    }
}
