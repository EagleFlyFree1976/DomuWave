using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.CondominiumInstallment;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetInstallmentByYearAndNumberCommandConsumer : InMemoryConsumerBase<GetInstallmentByYearAndNumberCommand, CondominiumInstallment>
{
    private readonly ICondominiumInstallmentService _condominiumInstallmentService;
    private readonly IUserService _userService;

    public GetInstallmentByYearAndNumberCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumInstallmentService condominiumInstallmentService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _condominiumInstallmentService = condominiumInstallmentService;
        _userService = userService;
    }

    protected override async Task<CondominiumInstallment> Consume(
        GetInstallmentByYearAndNumberCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _condominiumInstallmentService
            .GetByYearAndNumberAsync(command.CondominiumId, command.Year, command.Number, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
