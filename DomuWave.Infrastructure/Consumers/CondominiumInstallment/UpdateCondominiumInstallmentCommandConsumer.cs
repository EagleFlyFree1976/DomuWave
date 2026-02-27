using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.CondominiumInstallment;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateCondominiumInstallmentCommandConsumer : InMemoryConsumerBase<UpdateCondominiumInstallmentCommand, CondominiumInstallment>
{
    private readonly ICondominiumInstallmentService _condominiumInstallmentService;
    private readonly IUserService _userService;

    public UpdateCondominiumInstallmentCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumInstallmentService condominiumInstallmentService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _condominiumInstallmentService = condominiumInstallmentService;
        _userService = userService;
    }

    protected override async Task<CondominiumInstallment> Consume(
        UpdateCondominiumInstallmentCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var exists = await _condominiumInstallmentService
            .ExistsAsync(command.InstallmentId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (!exists) return null;
        command.Entity.Id = command.InstallmentId;
        return await _condominiumInstallmentService
            .UpdateAsync(command.Entity, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
