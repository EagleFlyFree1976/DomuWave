using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.MillesimalTable;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetActiveMillesimalTablesCommandConsumer : InMemoryConsumerBase<GetActiveMillesimalTablesCommand, IList<MillesimalTable>>
{
    private readonly IMillesimalTableService _millesimalTableService;
    private readonly IUserService _userService;

    public GetActiveMillesimalTablesCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IMillesimalTableService millesimalTableService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _millesimalTableService = millesimalTableService;
        _userService = userService;
    }

    protected override async Task<IList<MillesimalTable>> Consume(
        GetActiveMillesimalTablesCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _millesimalTableService
            .GetActiveTablesAsync(command.CondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
