using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UnitMillesimal;
using DomuWave.Services.Dto.UnitMillesimal;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetUnitMillesimalsByTableCommandConsumer
    : InMemoryConsumerBase<GetUnitMillesimalsByTableCommand, IList<UnitMillesimalReadDto>>
{
    private readonly IUnitMillesimalService _unitMillesimalService;
    private readonly IUserService           _userService;

    public GetUnitMillesimalsByTableCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUnitMillesimalService  unitMillesimalService,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _unitMillesimalService = unitMillesimalService;
        _userService           = userService;
    }

    protected override async Task<IList<UnitMillesimalReadDto>> Consume(
        GetUnitMillesimalsByTableCommand command,
        IMediationContext                mediationContext,
        CancellationToken               cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var entities = await _unitMillesimalService
            .GetByMillesimalTableIdAsync(command.MillesimalTableId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return entities.Select(e => e.ToReadDto()).ToList();
    }
}
