using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Condominium;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateCondominiumCommandConsumer : InMemoryConsumerBase<UpdateCondominiumCommand, CondominiumReadDto>
{
    private readonly ICondominiumService _condominiumService;
    private readonly IUserService _userService;

    public UpdateCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumService condominiumService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _condominiumService = condominiumService;
        _userService = userService;
    }

    protected override async Task<CondominiumReadDto> Consume(
        UpdateCondominiumCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var exists = await _condominiumService
            .ExistsAsync(command.CondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (!exists) return null;

        var existing = await _condominiumService
            .GetByIdAsync(command.CondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        existing.ApplyUpdate(command.Dto);

        var updated = await _condominiumService
            .UpdateAsync(existing, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return updated.ToReadDto();
    }
}
