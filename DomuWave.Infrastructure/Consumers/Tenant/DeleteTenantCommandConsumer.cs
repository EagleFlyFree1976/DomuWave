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
    private readonly ITenantService _tenantService;
    private readonly IUserService _userService;

    public DeleteTenantCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ITenantService tenantService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _tenantService = tenantService;
        _userService   = userService;
    }

    protected override async Task<bool> Consume(
        DeleteTenantCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _tenantService
            .DeleteAsync(command.TenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
