using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Dto.Tenant;

namespace DomuWave.Application.Models
{
    public class UserDto : CPQ.Core.DTO.UserDto
    {

        public UserProfile Profile { get; set; }
        public TenantReadDto? Tenant { get; set; }

        public IList<TenantReadDto> AvailableTenants { get; set; }

        /// <summary>
        /// Popolato solo per utenti Condomino (Profile == User):
        /// lista dei condomini presso cui l'utente risulta proprietario.
        /// </summary>
        public IList<CondominiumSummaryDto>? AvailableCondominiums { get; set; }
    }


    public enum UserProfile : int
    {
        SuperAdmin = 1,
        TenantAdministrator = 2,
        User = 3
    }
}
