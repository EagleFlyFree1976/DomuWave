using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.CondominiumFee;
using DomuWave.Services.Dto.CondominiumFee;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetFeesByInstallmentCommandConsumer : InMemoryConsumerBase<GetFeesByInstallmentCommand, IList<CondominiumFeeReadDto>>
{
    private readonly ICondominiumFeeService _condominiumFeeService;
    private readonly IUserService           _userService;
    private readonly IUserTenantService     _userTenantService;

    public GetFeesByInstallmentCommandConsumer(
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
        GetFeesByInstallmentCommand command,
        IMediationContext            mediationContext,
        CancellationToken            cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var entities = await _condominiumFeeService
            .GetByInstallmentIdAsync(command.InstallmentId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // Condòmino NEL TENANT ATTIVO: vede solo le proprie quote (filtra per UserId).
        var isCondomino = await _userTenantService
            .IsCondominoInTenantAsync(command.CurrentUserId, command.TenantId, cancellationToken)
            .ConfigureAwait(false);
        if (isCondomino)
        {
            long userId = command.CurrentUserId;
            entities = entities.Where(e => e.UserId == userId).ToList();
        }

        return entities.Select(e => e.ToReadDto()).ToList();
    }
}
