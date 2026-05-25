using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UserTenant;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateUserTenantCommandConsumer : InMemoryConsumerBase<CreateUserTenantCommand, UserTenant>
{
    private readonly IUserTenantService _userTenantService;
    private readonly IUserService       _userService;
    private readonly ITenantAccessCache _accessCache;

    public CreateUserTenantCommandConsumer(
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
        CreateUserTenantCommand command,
        IMediationContext        mediationContext,
        CancellationToken        cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var created = await _userTenantService
            .CreateAsync(command.Entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // Nuovo accesso utente-tenant: invalida entry precedente (eventualmente cached come false)
        if (created.Tenant?.Id is Guid tenantId)
            _accessCache.InvalidateEntry(created.UserId, tenantId);

        return created;
    }
}
