using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Condominium;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

/// <summary>
/// Aggiornamento mirato del solo saldo iniziale di cassa del condominio,
/// senza richiedere l'intero payload di update (usato dal Rendiconto).
/// </summary>
public class UpdateCondominiumInitialBalanceCommandConsumer
    : InMemoryConsumerBase<UpdateCondominiumInitialBalanceCommand, CondominiumReadDto>
{
    private readonly ICondominiumService _condominiumService;
    private readonly IUserService        _userService;

    public UpdateCondominiumInitialBalanceCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumService     condominiumService,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _condominiumService = condominiumService;
        _userService        = userService;
    }

    protected override async Task<CondominiumReadDto> Consume(
        UpdateCondominiumInitialBalanceCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var existing = await _condominiumService
            .GetByIdAsync(command.CondominiumId, command.TenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (existing == null) return null;

        existing.InitialBalance = command.InitialBalance;

        var updated = await _condominiumService
            .UpdateAsync(existing, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return updated.ToReadDto();
    }
}
