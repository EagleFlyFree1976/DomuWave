using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UserTenant;
using DomuWave.Services.Dto.UserTenants;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetTenantProfileCommandConsumer
    : InMemoryConsumerBase<GetTenantProfileCommand, TenantProfileDto>
{
    // Allineati a DomuWave.Application.Models.UserProfile (non referenziabile da qui).
    private const int ProfileTenantAdministrator = 2;
    private const int ProfileCondomino           = 3;

    private readonly IUserTenantService _userTenantService;
    private readonly IUserService       _userService;

    public GetTenantProfileCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserTenantService userTenantService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userTenantService = userTenantService;
        _userService       = userService;
    }

    protected override async Task<TenantProfileDto> Consume(
        GetTenantProfileCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var roleCode = await _userTenantService
            .GetRoleCodeForTenantAsync(command.CurrentUserId, command.TenantId, cancellationToken)
            .ConfigureAwait(false);

        // Fallback legacy: nessuna associazione esplicita → ruolo globale.
        var isCondomino = roleCode != null
            ? string.Equals(roleCode, "Condomino", StringComparison.OrdinalIgnoreCase)
            : string.Equals(currentUser.Role?.Code, "condomino", StringComparison.OrdinalIgnoreCase);

        var dto = new TenantProfileDto
        {
            RoleCode = roleCode ?? currentUser.Role?.Code,
            Profile  = isCondomino ? ProfileCondomino : ProfileTenantAdministrator,
        };

        if (isCondomino)
        {
            var all = await _userTenantService
                .GetCondominiumsByCondominoUserIdAsync(command.CurrentUserId, currentUser, cancellationToken)
                .ConfigureAwait(false);
            dto.Condominiums = all.Where(c => c.TenantId == command.TenantId).ToList();
        }

        return dto;
    }
}
