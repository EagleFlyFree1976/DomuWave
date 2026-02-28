using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UserTenant;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetUserTenantsByTenantCommandConsumer : InMemoryConsumerBase<GetUserTenantsByTenantCommand, IList<UserTenant>>
{
    private readonly IUserTenantService _userTenantService;
    private readonly IUserService _userService;

    public GetUserTenantsByTenantCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserTenantService userTenantService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userTenantService = userTenantService;
        _userService = userService;
    }

    protected override async Task<IList<UserTenant>> Consume(
        GetUserTenantsByTenantCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _userTenantService
            .GetByTenantIdAsync(command.TenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
