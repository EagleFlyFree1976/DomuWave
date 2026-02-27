using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Tenant;
using DomuWave.Services.Dto.Tenant;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Tenant;

public class GetAllTenantsCommandConsumer
    : InMemoryConsumerBase<GetAllTenantsCommand, IList<TenantReadDto>>
{
    private readonly ITenantService _tenantService;
    private readonly IUserService _userService;

    public GetAllTenantsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ITenantService tenantService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _tenantService = tenantService;
        _userService   = userService;
    }

    protected override async Task<IList<TenantReadDto>> Consume(
        GetAllTenantsCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var tenants = await _tenantService
            .GetAllAsync(currentUser, cancellationToken)
            .ConfigureAwait(false);

        return tenants.Select(t => t.ToReadDto()).ToList();
    }
}
