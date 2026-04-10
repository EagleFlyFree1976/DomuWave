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

public class GetConsumptionTypesByCondominiumCommandConsumer
    : InMemoryConsumerBase<GetConsumptionTypesByCondominiumCommand, IList<ConsumptionTypeReadDto>>
{
    private readonly IUserService _userService;
    public GetConsumptionTypesByCondominiumCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<IList<ConsumptionTypeReadDto>> Consume(GetConsumptionTypesByCondominiumCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var items = await session.Query<ConsumptionType>()
            .Where(x => x.Condominium.Id == command.CondominiumId)
            .OrderBy(x => x.IsDeleted).ThenBy(x => x.Name)
            .ToListAsync(ct).ConfigureAwait(false);
        return items.Select(x => x.ToReadDto()).ToList();
    }
}

public class GetConsumptionTypeByIdCommandConsumer
    : InMemoryConsumerBase<GetConsumptionTypeByIdCommand, ConsumptionTypeReadDto>
{
    private readonly IUserService _userService;
    public GetConsumptionTypeByIdCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<ConsumptionTypeReadDto> Consume(GetConsumptionTypeByIdCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var item = await session.Query<ConsumptionType>()
            .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct).ConfigureAwait(false);
        return item?.ToReadDto();
    }
}

public class CreateConsumptionTypeCommandConsumer
    : InMemoryConsumerBase<CreateConsumptionTypeCommand, ConsumptionTypeReadDto>
{
    private readonly IUserService _userService;
    public CreateConsumptionTypeCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<ConsumptionTypeReadDto> Consume(CreateConsumptionTypeCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);

        var condominium = await session.Query<Models.Condominium>()
            .FirstOrDefaultAsync(x => x.Id == command.Dto.CondominiumId && !x.IsDeleted, ct).ConfigureAwait(false);
        if (condominium == null) throw new NotFoundException("Condominio non trovato.");

        if (string.IsNullOrWhiteSpace(command.Dto.Name))
            throw new ValidatorException("Il nome è obbligatorio.");

        var entity = command.Dto.ToEntity(condominium, condominium.Tenant);
        entity.Trace(currentUser);
        await session.SaveAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}

public class UpdateConsumptionTypeCommandConsumer
    : InMemoryConsumerBase<UpdateConsumptionTypeCommand, ConsumptionTypeReadDto>
{
    private readonly IUserService _userService;
    public UpdateConsumptionTypeCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<ConsumptionTypeReadDto> Consume(UpdateConsumptionTypeCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);
        var entity = await session.Query<ConsumptionType>()
            .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct).ConfigureAwait(false);
        if (entity == null) throw new NotFoundException("Tipo consumo non trovato.");
        entity.ApplyUpdate(command.Dto);
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}

public class DeleteConsumptionTypeCommandConsumer
    : InMemoryConsumerBase<DeleteConsumptionTypeCommand, bool>
{
    private readonly IUserService _userService;
    public DeleteConsumptionTypeCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<bool> Consume(DeleteConsumptionTypeCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);
        var entity = await session.Query<ConsumptionType>()
            .FirstOrDefaultAsync(x => x.Id == command.Id, ct).ConfigureAwait(false);
        if (entity == null) return false;

        // Verifica se esistono letture in ripartizioni approvate per questo tipo
        var hasApprovedReadings = await session.Query<ConsumptionReading>()
            .AnyAsync(r => r.Meter.ConsumptionType.Id == command.Id
                        && r.Charge != null
                        && r.Charge.Status.Id == ConsumptionChargeStatus.Approved
                        && !r.IsDeleted, ct).ConfigureAwait(false);

        if (hasApprovedReadings)
        {
            // Cancellazione logica: il tipo rimane visibile (evidenziato) per storico
            entity.IsDeleted = true;
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        }
        else
        {
            // Cancellazione fisica a cascata: letture → contatori → tipo
            var readings = await session.Query<ConsumptionReading>()
                .Where(r => r.Meter.ConsumptionType.Id == command.Id)
                .ToListAsync(ct).ConfigureAwait(false);
            foreach (var r in readings)
                await session.DeleteAsync(r, ct).ConfigureAwait(false);

            var meters = await session.Query<Meter>()
                .Where(m => m.ConsumptionType.Id == command.Id)
                .ToListAsync(ct).ConfigureAwait(false);
            foreach (var m in meters)
                await session.DeleteAsync(m, ct).ConfigureAwait(false);

            await session.DeleteAsync(entity, ct).ConfigureAwait(false);
        }

        await session.FlushAsync(ct).ConfigureAwait(false);
        return true;
    }
}
