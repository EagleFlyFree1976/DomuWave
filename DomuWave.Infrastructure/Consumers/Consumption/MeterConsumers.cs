using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Consumption;
using DomuWave.Services.Dto.Consumption;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetMetersByCondominiumCommandConsumer
    : InMemoryConsumerBase<GetMetersByCondominiumCommand, IList<MeterReadDto>>
{
    private readonly IUserService _userService;
    public GetMetersByCondominiumCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<IList<MeterReadDto>> Consume(GetMetersByCondominiumCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var items = await session.Query<Meter>()
            .Where(x => x.ConsumptionType.Condominium.Id == command.CondominiumId)
            .OrderBy(x => x.IsDeleted).ThenBy(x => x.ConsumptionType.IsDeleted).ThenBy(x => x.ConsumptionType.Name).ThenBy(x => x.Unit.InternalNumber)
            .ToListAsync(ct).ConfigureAwait(false);
        return items.Select(x => x.ToReadDto()).ToList();
    }
}

public class GetMetersByConsumptionTypeCommandConsumer
    : InMemoryConsumerBase<GetMetersByConsumptionTypeCommand, IList<MeterReadDto>>
{
    private readonly IUserService _userService;
    public GetMetersByConsumptionTypeCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<IList<MeterReadDto>> Consume(GetMetersByConsumptionTypeCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var items = await session.Query<Meter>()
            .Where(x => x.ConsumptionType.Id == command.ConsumptionTypeId)
            .OrderBy(x => x.IsDeleted).ThenBy(x => x.Unit.InternalNumber)
            .ToListAsync(ct).ConfigureAwait(false);
        return items.Select(x => x.ToReadDto()).ToList();
    }
}

public class GetMeterByIdCommandConsumer
    : InMemoryConsumerBase<GetMeterByIdCommand, MeterReadDto>
{
    private readonly IUserService _userService;
    public GetMeterByIdCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<MeterReadDto> Consume(GetMeterByIdCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var item = await session.Query<Meter>()
            .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct).ConfigureAwait(false);
        return item?.ToReadDto();
    }
}

public class CreateMeterCommandConsumer
    : InMemoryConsumerBase<CreateMeterCommand, MeterReadDto>
{
    private readonly IUserService _userService;
    public CreateMeterCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<MeterReadDto> Consume(CreateMeterCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);

        var type = await session.Query<ConsumptionType>()
            .FirstOrDefaultAsync(x => x.Id == command.Dto.ConsumptionTypeId && !x.IsDeleted, ct).ConfigureAwait(false);
        if (type == null) throw new NotFoundException("Tipo consumo non trovato.");

        var unit = await session.Query<RealEstateUnit>()
            .FirstOrDefaultAsync(x => x.Id == command.Dto.UnitId && !x.IsDeleted, ct).ConfigureAwait(false);
        if (unit == null) throw new NotFoundException("Unità immobiliare non trovata.");

        if (string.IsNullOrWhiteSpace(command.Dto.Code))
            throw new ValidatorException("Il codice contatore è obbligatorio.");

        var entity = command.Dto.ToEntity(type, unit, type.Tenant);
        entity.Trace(currentUser);
        await session.SaveAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}

public class UpdateMeterCommandConsumer
    : InMemoryConsumerBase<UpdateMeterCommand, MeterReadDto>
{
    private readonly IUserService _userService;
    public UpdateMeterCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<MeterReadDto> Consume(UpdateMeterCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);
        var entity = await session.Query<Meter>()
            .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct).ConfigureAwait(false);
        if (entity == null) throw new NotFoundException("Contatore non trovato.");
        entity.ApplyUpdate(command.Dto);
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}

public class DeleteMeterCommandConsumer
    : InMemoryConsumerBase<DeleteMeterCommand, bool>
{
    private readonly IUserService _userService;
    public DeleteMeterCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<bool> Consume(DeleteMeterCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);
        var entity = await session.Query<Meter>()
            .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct).ConfigureAwait(false);
        if (entity == null) return false;
        entity.IsDeleted = true;
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return true;
    }
}
