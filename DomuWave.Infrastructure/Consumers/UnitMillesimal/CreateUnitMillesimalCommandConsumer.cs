using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UnitMillesimal;
using DomuWave.Services.Dto.UnitMillesimal;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateUnitMillesimalCommandConsumer
    : InMemoryConsumerBase<CreateUnitMillesimalCommand, UnitMillesimalReadDto>
{
    private readonly IUnitMillesimalService _unitMillesimalService;
    private readonly IUserService           _userService;

    public CreateUnitMillesimalCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUnitMillesimalService  unitMillesimalService,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _unitMillesimalService = unitMillesimalService;
        _userService           = userService;
    }

    protected override async Task<UnitMillesimalReadDto> Consume(
        CreateUnitMillesimalCommand command,
        IMediationContext            mediationContext,
        CancellationToken           cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var dto = command.Dto;

        var table = await session.Query<MillesimalTable>()
            .FirstOrDefaultAsync(x => x.Id == dto.MillesimalTableId && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (table == null)
            throw new NotFoundException("Tabella millesimale non trovata.");

        var unit = await session.Query<RealEstateUnit>()
            .FirstOrDefaultAsync(x => x.Id == dto.UnitId && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (unit == null)
            throw new NotFoundException("Unità immobiliare non trovata.");

        var duplicate = await session.Query<UnitMillesimal>()
            .AnyAsync(x => x.MillesimalTable.Id == dto.MillesimalTableId
                        && x.Unit.Id            == dto.UnitId
                        && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (duplicate)
            throw new ValidatorException("Esiste già una voce millesimale per questa unità in questa tabella.");

        var entity  = dto.ToEntity(table, unit);
        var created = await _unitMillesimalService
            .CreateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return created.ToReadDto();
    }
}
