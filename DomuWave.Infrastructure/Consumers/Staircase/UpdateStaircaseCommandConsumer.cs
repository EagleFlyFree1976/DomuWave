using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Staircase;
using DomuWave.Services.Dto.Staircase;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateStaircaseCommandConsumer : InMemoryConsumerBase<UpdateStaircaseCommand, StaircaseReadDto>
{
    private readonly IStaircaseService _staircaseService;
    private readonly IUserService      _userService;

    public UpdateStaircaseCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IStaircaseService staircaseService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _staircaseService = staircaseService;
        _userService      = userService;
    }

    protected override async Task<StaircaseReadDto> Consume(
        UpdateStaircaseCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(command.Dto.Name))
            throw new ValidatorException("Il nome della scala è obbligatorio.");

        var entity = await _staircaseService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (entity == null) return null;

        var duplicate = await session.Query<Staircase>()
            .AnyAsync(x => x.Condominium.Id == entity.Condominium.Id
                        && x.Name == command.Dto.Name.Trim()
                        && x.Id   != command.Id
                        && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (duplicate)
            throw new ValidatorException($"Esiste già una scala '{command.Dto.Name}' in questo condominio.");

        entity.ApplyUpdate(command.Dto);
        entity.Trace(currentUser);

        var updated = await _staircaseService
            .UpdateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return updated.ToReadDto();
    }
}
