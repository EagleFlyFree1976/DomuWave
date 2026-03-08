using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.CondominiumInstallment;
using DomuWave.Services.Dto.CondominiumInstallment;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetInstallmentsByFiscalYearCommandConsumer : InMemoryConsumerBase<GetInstallmentsByFiscalYearCommand, IList<CondominiumInstallmentReadDto>>
{
    private readonly ICondominiumInstallmentService _condominiumInstallmentService;
    private readonly IUserService                   _userService;

    public GetInstallmentsByFiscalYearCommandConsumer(
        ISessionFactoryProvider        sessionFactoryProvider,
        ICondominiumInstallmentService condominiumInstallmentService,
        IUserService                   userService) : base(sessionFactoryProvider)
    {
        _condominiumInstallmentService = condominiumInstallmentService;
        _userService                   = userService;
    }

    protected override async Task<IList<CondominiumInstallmentReadDto>> Consume(
        GetInstallmentsByFiscalYearCommand command,
        IMediationContext                  mediationContext,
        CancellationToken                  cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var entities = await _condominiumInstallmentService
            .GetByFiscalYearIdAsync(command.CondominiumId, command.FiscalYearId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return entities.Select(e => e.ToReadDto()).ToList();
    }
}
