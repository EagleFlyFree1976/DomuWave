using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.CondominiumFee;
using DomuWave.Services.Dto.CondominiumFee;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetFeesByUserCommandConsumer : InMemoryConsumerBase<GetFeesByUserCommand, IList<CondominiumFeeReadDto>>
{
    private readonly ICondominiumFeeService _condominiumFeeService;
    private readonly IUserService           _userService;
    private readonly IUserTenantService     _userTenantService;

    public GetFeesByUserCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumFeeService  condominiumFeeService,
        IUserService            userService,
        IUserTenantService      userTenantService) : base(sessionFactoryProvider)
    {
        _condominiumFeeService = condominiumFeeService;
        _userService           = userService;
        _userTenantService     = userTenantService;
    }

    protected override async Task<IList<CondominiumFeeReadDto>> Consume(
        GetFeesByUserCommand command,
        IMediationContext    mediationContext,
        CancellationToken    cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        // Condòmino NEL TENANT ATTIVO: può richiedere solo le proprie quote.
        var targetUserId = command.UserId;
        var isCondomino = await _userTenantService
            .IsCondominoInTenantAsync(command.CurrentUserId, command.TenantId, cancellationToken)
            .ConfigureAwait(false);
        if (isCondomino)
            targetUserId = command.CurrentUserId;

        var entities = await _condominiumFeeService
            .GetByUserIdAsync(targetUserId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return entities.Select(e => e.ToReadDto()).ToList();
    }
}
