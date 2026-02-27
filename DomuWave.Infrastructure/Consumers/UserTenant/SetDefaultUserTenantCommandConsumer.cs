using DomuWave.Services.Models;
using DomuWave.Services.Dto.UserTenants;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UserTenant;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class SetDefaultUserTenantCommandConsumer : InMemoryConsumerBase<SetDefaultUserTenantCommand, UserTenant>
{
    private readonly IUserTenantService _userTenantService;
    private readonly IUserService _userService;

    public SetDefaultUserTenantCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserTenantService userTenantService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userTenantService = userTenantService;
        _userService = userService;
    }

    protected override async Task<UserTenant> Consume(
        SetDefaultUserTenantCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _userTenantService
            .SetDefaultAsync(command.UserId, command.UserTenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
