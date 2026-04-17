using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.CondominiumInstallment;
using DomuWave.Services.Dto.CondominiumInstallment;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetInstallmentsByCondominiumCommandConsumer : InMemoryConsumerBase<GetInstallmentsByCondominiumCommand, IList<CondominiumInstallmentReadDto>>
{
    private readonly ICondominiumInstallmentService _condominiumInstallmentService;
    private readonly IUserService                   _userService;

    public GetInstallmentsByCondominiumCommandConsumer(
        ISessionFactoryProvider        sessionFactoryProvider,
        ICondominiumInstallmentService condominiumInstallmentService,
        IUserService                   userService) : base(sessionFactoryProvider)
    {
        _condominiumInstallmentService = condominiumInstallmentService;
        _userService                   = userService;
    }

    protected override async Task<IList<CondominiumInstallmentReadDto>> Consume(
        GetInstallmentsByCondominiumCommand command,
        IMediationContext                   mediationContext,
        CancellationToken                   cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var entities = await _condominiumInstallmentService
            .GetByCondominiumIdAsync(command.CondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        var dtos = entities.Select(e => e.ToReadDto()).ToList();
        await InstallmentFeeEnricher.EnrichAsync(session, dtos, cancellationToken).ConfigureAwait(false);
        return dtos;
    }
}
