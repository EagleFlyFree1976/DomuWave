using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.MillesimalTable;
using DomuWave.Services.Dto.MillesimalTable;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetMillesimalTableByIdCommandConsumer : InMemoryConsumerBase<GetMillesimalTableByIdCommand, MillesimalTableReadDto>
{
    private readonly IMillesimalTableService _millesimalTableService;
    private readonly IUserService            _userService;

    public GetMillesimalTableByIdCommandConsumer(
        ISessionFactoryProvider  sessionFactoryProvider,
        IMillesimalTableService  millesimalTableService,
        IUserService             userService) : base(sessionFactoryProvider)
    {
        _millesimalTableService = millesimalTableService;
        _userService            = userService;
    }

    protected override async Task<MillesimalTableReadDto> Consume(
        GetMillesimalTableByIdCommand command,
        IMediationContext              mediationContext,
        CancellationToken              cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var entity = await _millesimalTableService
            .GetByIdAsync(command.TableId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        if (entity == null) return null;

        var calculated = await session.Query<UnitMillesimal>()
            .Where(x => x.MillesimalTable.Id == entity.Id && !x.IsDeleted)
            .SumAsync(x => (decimal?)x.Millesimal, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        return entity.ToReadDto(calculated);
    }
}
