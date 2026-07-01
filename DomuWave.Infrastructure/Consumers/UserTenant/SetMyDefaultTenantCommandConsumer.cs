using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UserTenant;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class SetMyDefaultTenantCommandConsumer : InMemoryConsumerBase<SetMyDefaultTenantCommand, bool>
{
    private readonly IUserTenantService _userTenantService;
    private readonly IUserService       _userService;

    public SetMyDefaultTenantCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserTenantService userTenantService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userTenantService = userTenantService;
        _userService       = userService;
    }

    protected override async Task<bool> Consume(
        SetMyDefaultTenantCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var result = await _userTenantService
            .SetDefaultByTenantAsync(command.CurrentUserId, command.TenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return result != null;
    }
}
