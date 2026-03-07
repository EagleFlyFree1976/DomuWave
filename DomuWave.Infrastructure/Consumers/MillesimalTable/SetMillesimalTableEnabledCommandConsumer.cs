using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.MillesimalTable;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class SetMillesimalTableEnabledCommandConsumer : InMemoryConsumerBase<SetMillesimalTableEnabledCommand, bool>
{
    private readonly IMillesimalTableService _millesimalTableService;
    private readonly IUserService            _userService;

    public SetMillesimalTableEnabledCommandConsumer(
        ISessionFactoryProvider  sessionFactoryProvider,
        IMillesimalTableService  millesimalTableService,
        IUserService             userService) : base(sessionFactoryProvider)
    {
        _millesimalTableService = millesimalTableService;
        _userService            = userService;
    }

    protected override async Task<bool> Consume(
        SetMillesimalTableEnabledCommand command,
        IMediationContext                mediationContext,
        CancellationToken                cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var entity = await _millesimalTableService
            .GetByIdAsync(command.TableId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (entity == null)
            throw new NotFoundException("Tabella millesimale non trovata.");

        entity.IsEnabled = command.IsEnabled;

        await _millesimalTableService
            .UpdateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return true;
    }
}
