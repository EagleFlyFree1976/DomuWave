using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.MillesimalTable;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateMillesimalTableCommandConsumer : InMemoryConsumerBase<UpdateMillesimalTableCommand, Models.MillesimalTable>
{
    private readonly IMillesimalTableService _millesimalTableService;
    private readonly IUserService _userService;

    public UpdateMillesimalTableCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IMillesimalTableService millesimalTableService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _millesimalTableService = millesimalTableService;
        _userService = userService;
    }

    protected override async Task<Models.MillesimalTable> Consume(
        UpdateMillesimalTableCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var exists = await _millesimalTableService
            .ExistsAsync(command.TableId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (!exists) return null;
        command.Entity.Id = command.TableId;
        return await _millesimalTableService
            .UpdateAsync(command.Entity, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
