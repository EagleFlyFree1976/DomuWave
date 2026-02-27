using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Tenant;
using DomuWave.Services.Dto.Tenant;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Tenant;

public class GetActiveTenantsCommandConsumer
    : InMemoryConsumerBase<GetActiveTenantsCommand, IList<TenantReadDto>>
{
    private readonly ITenantService _tenantService;
    private readonly IUserService _userService;

    public GetActiveTenantsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ITenantService tenantService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _tenantService = tenantService;
        _userService   = userService;
    }

    protected override async Task<IList<TenantReadDto>> Consume(
        GetActiveTenantsCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var tenants = await _tenantService
            .GetActiveTenantsAsync(currentUser, cancellationToken)
            .ConfigureAwait(false);

        return tenants.Select(t => t.ToReadDto()).ToList();
    }
}
