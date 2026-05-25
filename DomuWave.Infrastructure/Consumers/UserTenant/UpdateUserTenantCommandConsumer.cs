using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UserTenant;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateUserTenantCommandConsumer : InMemoryConsumerBase<UpdateUserTenantCommand, UserTenant>
{
    private readonly IUserTenantService _userTenantService;
    private readonly IUserService       _userService;
    private readonly ITenantAccessCache _accessCache;

    public UpdateUserTenantCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserTenantService      userTenantService,
        IUserService            userService,
        ITenantAccessCache      accessCache) : base(sessionFactoryProvider)
    {
        _userTenantService = userTenantService;
        _userService       = userService;
        _accessCache       = accessCache;
    }

    protected override async Task<UserTenant> Consume(
        UpdateUserTenantCommand command,
        IMediationContext        mediationContext,
        CancellationToken        cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var existing = await _userTenantService
            .GetByIdAsync(command.UserTenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (existing == null) return null;

        // Invalida prima dell'update: IsActive potrebbe cambiare
        if (existing.Tenant?.Id is Guid tenantId)
            _accessCache.InvalidateEntry(existing.UserId, tenantId);

        command.Entity.Id = command.UserTenantId;
        return await _userTenantService
            .UpdateAsync(command.Entity, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
