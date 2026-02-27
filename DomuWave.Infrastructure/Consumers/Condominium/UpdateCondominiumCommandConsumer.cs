using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Condominium;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateCondominiumCommandConsumer : InMemoryConsumerBase<UpdateCondominiumCommand, Condominium>
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

    protected override async Task<Condominium> Consume(
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
        command.Entity.Id = command.CondominiumId;
        return await _condominiumService
            .UpdateAsync(command.Entity, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
