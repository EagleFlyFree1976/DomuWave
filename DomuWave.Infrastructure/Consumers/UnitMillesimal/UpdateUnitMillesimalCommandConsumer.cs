using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UnitMillesimal;
using DomuWave.Services.Dto.UnitMillesimal;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateUnitMillesimalCommandConsumer
    : InMemoryConsumerBase<UpdateUnitMillesimalCommand, UnitMillesimalReadDto>
{
    private readonly IUnitMillesimalService _unitMillesimalService;
    private readonly IUserService           _userService;

    public UpdateUnitMillesimalCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUnitMillesimalService  unitMillesimalService,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _unitMillesimalService = unitMillesimalService;
        _userService           = userService;
    }

    protected override async Task<UnitMillesimalReadDto> Consume(
        UpdateUnitMillesimalCommand command,
        IMediationContext            mediationContext,
        CancellationToken           cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var entity = await _unitMillesimalService
            .GetByIdAsync(command.EntryId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        if (entity == null) return null;

        entity.ApplyUpdate(command.Dto);
        var updated = await _unitMillesimalService
            .UpdateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return updated.ToReadDto();
    }
}
