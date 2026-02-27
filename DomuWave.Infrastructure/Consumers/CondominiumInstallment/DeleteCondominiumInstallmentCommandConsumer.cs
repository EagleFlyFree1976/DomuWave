using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.CondominiumInstallment;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteCondominiumInstallmentCommandConsumer : InMemoryConsumerBase<DeleteCondominiumInstallmentCommand, bool>
{
    private readonly ICondominiumInstallmentService _condominiumInstallmentService;
    private readonly IUserService _userService;

    public DeleteCondominiumInstallmentCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumInstallmentService condominiumInstallmentService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _condominiumInstallmentService = condominiumInstallmentService;
        _userService = userService;
    }

    protected override async Task<bool> Consume(
        DeleteCondominiumInstallmentCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _condominiumInstallmentService
            .DeleteAsync(command.InstallmentId, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
