using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UserTenant;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteUserTenantCommandConsumer : InMemoryConsumerBase<DeleteUserTenantCommand, bool>
{
    private readonly IUserTenantService _userTenantService;
    private readonly IUserService       _userService;
    private readonly ITenantAccessCache _accessCache;

    public DeleteUserTenantCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserTenantService      userTenantService,
        IUserService            userService,
        ITenantAccessCache      accessCache) : base(sessionFactoryProvider)
    {
        _userTenantService = userTenantService;
        _userService       = userService;
        _accessCache       = accessCache;
    }

    protected override async Task<bool> Consume(
        DeleteUserTenantCommand command,
        IMediationContext        mediationContext,
        CancellationToken        cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        // Leggi prima di eliminare per recuperare UserId e TenantId
        var existing = await _userTenantService
            .GetByIdAsync(command.UserTenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        var deleted = await _userTenantService
            .DeleteAsync(command.UserTenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        if (deleted && existing?.Tenant?.Id is Guid tenantId)
            _accessCache.InvalidateEntry(existing.UserId, tenantId);

        return deleted;
    }
}
