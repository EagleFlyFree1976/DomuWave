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
            .Where(x => x.Condominium.Id == command.CondominiumId && !x.IsDeleted)
            .OrderBy(x => x.Name)
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

        var condominium = await session.Query<Condominium>()
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
        entity.TraceUpdate(currentUser);
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
            .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct).ConfigureAwait(false);
        if (entity == null) return false;
        entity.IsDeleted = true;
        entity.TraceUpdate(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return true;
    }
}
