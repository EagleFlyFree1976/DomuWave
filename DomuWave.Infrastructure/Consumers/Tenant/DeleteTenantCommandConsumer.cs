using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Tenant;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Tenant;

public class DeleteTenantCommandConsumer
    : InMemoryConsumerBase<DeleteTenantCommand, bool>
{
    private readonly ITenantService     _tenantService;
    private readonly IUserService       _userService;
    private readonly ITenantAccessCache _accessCache;

    public DeleteTenantCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ITenantService          tenantService,
        IUserService            userService,
        ITenantAccessCache      accessCache) : base(sessionFactoryProvider)
    {
        _tenantService = tenantService;
        _userService   = userService;
        _accessCache   = accessCache;
    }

    protected override async Task<bool> Consume(
        DeleteTenantCommand command,
        IMediationContext   mediationContext,
        CancellationToken   cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var deleted = await _tenantService
            .DeleteAsync(command.TenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        if (deleted)
            _accessCache.InvalidateForTenant(command.TenantId);

        return deleted;
    }
}
