using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.CondominiumInstallment;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetOverdueInstallmentsCommandConsumer : InMemoryConsumerBase<GetOverdueInstallmentsCommand, IList<CondominiumInstallment>>
{
    private readonly ICondominiumInstallmentService _condominiumInstallmentService;
    private readonly IUserService _userService;

    public GetOverdueInstallmentsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumInstallmentService condominiumInstallmentService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _condominiumInstallmentService = condominiumInstallmentService;
        _userService = userService;
    }

    protected override async Task<IList<CondominiumInstallment>> Consume(
        GetOverdueInstallmentsCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _condominiumInstallmentService
            .GetOverdueInstallmentsAsync(command.CondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
