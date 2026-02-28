using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UnitTenant;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteUnitTenantCommandConsumer : InMemoryConsumerBase<DeleteUnitTenantCommand, bool>
{
    private readonly IUnitTenantService _unitTenantService;
    private readonly IUserService       _userService;

    public DeleteUnitTenantCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUnitTenantService unitTenantService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _unitTenantService = unitTenantService;
        _userService       = userService;
    }

    protected override async Task<bool> Consume(
        DeleteUnitTenantCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _unitTenantService
            .DeleteAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
