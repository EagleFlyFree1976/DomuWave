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

public class GetConsumptionReadingsByFiscalYearCommandConsumer
    : InMemoryConsumerBase<GetConsumptionReadingsByFiscalYearCommand, IList<ConsumptionReadingReadDto>>
{
    private readonly IUserService _userService;
    public GetConsumptionReadingsByFiscalYearCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<IList<ConsumptionReadingReadDto>> Consume(GetConsumptionReadingsByFiscalYearCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var items = await session.Query<ConsumptionReading>()
            .Where(x => x.Meter.ConsumptionType.Id == command.ConsumptionTypeId
                     && x.FiscalYear.Id == command.FiscalYearId
                     && !x.IsDeleted)
            .OrderBy(x => x.Meter.Unit.InternalNumber)
            .ToListAsync(ct).ConfigureAwait(false);
        return items.Select(x => x.ToReadDto()).ToList();
    }
}

public class GetConsumptionReadingByIdCommandConsumer
    : InMemoryConsumerBase<GetConsumptionReadingByIdCommand, ConsumptionReadingReadDto>
{
    private readonly IUserService _userService;
    public GetConsumptionReadingByIdCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<ConsumptionReadingReadDto> Consume(GetConsumptionReadingByIdCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var item = await session.Query<ConsumptionReading>()
            .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct).ConfigureAwait(false);
        return item?.ToReadDto();
    }
}

/// <summary>
/// Salva le letture per tutti i contatori di un tipo consumo + esercizio in una sola operazione (upsert).
/// </summary>
public class SaveConsumptionReadingsBulkCommandConsumer
    : InMemoryConsumerBase<SaveConsumptionReadingsBulkCommand, IList<ConsumptionReadingReadDto>>
{
    private readonly IUserService _userService;
    public SaveConsumptionReadingsBulkCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<IList<ConsumptionReadingReadDto>> Consume(SaveConsumptionReadingsBulkCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);

        var consumptionType = await session.Query<ConsumptionType>()
            .FirstOrDefaultAsync(x => x.Id == command.ConsumptionTypeId && !x.IsDeleted, ct).ConfigureAwait(false);
        if (consumptionType == null) throw new NotFoundException("Tipo consumo non trovato.");

        var fiscalYear = await session.Query<FiscalYear>()
            .FirstOrDefaultAsync(x => x.Id == command.FiscalYearId && !x.IsDeleted, ct).ConfigureAwait(false);
        if (fiscalYear == null) throw new NotFoundException("Esercizio non trovato.");

        var meterIds = command.Items.Select(i => i.MeterId).ToList();

        var meters = await session.Query<Meter>()
            .Where(m => meterIds.Contains(m.Id) && !m.IsDeleted)
            .ToListAsync(ct).ConfigureAwait(false);

        var existingReadings = await session.Query<ConsumptionReading>()
            .Where(r => meterIds.Contains(r.Meter.Id) && r.FiscalYear.Id == command.FiscalYearId && !r.IsDeleted)
            .ToListAsync(ct).ConfigureAwait(false);

        var readingByMeterId = existingReadings.ToDictionary(r => r.Meter.Id);
        var meterById = meters.ToDictionary(m => m.Id);

        var result = new List<ConsumptionReading>();

        foreach (var item in command.Items)
        {
            if (!meterById.TryGetValue(item.MeterId, out var meter)) continue;

            if (readingByMeterId.TryGetValue(item.MeterId, out var reading))
            {
                reading.InitialDate  = item.InitialDate;
                reading.InitialValue = item.InitialValue;
                reading.FinalDate    = item.FinalDate;
                reading.FinalValue   = item.FinalValue;
                reading.Notes        = item.Notes;
                reading.TraceUpdate(currentUser);
            }
            else
            {
                reading = new ConsumptionReading
                {
                    Meter        = meter,
                    FiscalYear   = fiscalYear,
                    Tenant       = meter.Tenant,
                    InitialDate  = item.InitialDate,
                    InitialValue = item.InitialValue,
                    FinalDate    = item.FinalDate,
                    FinalValue   = item.FinalValue,
                    Notes        = item.Notes,
                    IsDeleted    = false,
                };
                reading.Trace(currentUser);
            }

            await session.SaveOrUpdateAsync(reading, ct).ConfigureAwait(false);
            result.Add(reading);
        }

        await session.FlushAsync(ct).ConfigureAwait(false);

        // Rilegge per avere Consumption calcolato dal DB
        await session.RefreshAsync(result.LastOrDefault(), ct).ConfigureAwait(false);

        return result.Select(x => x.ToReadDto()).ToList();
    }
}

public class DeleteConsumptionReadingCommandConsumer
    : InMemoryConsumerBase<DeleteConsumptionReadingCommand, bool>
{
    private readonly IUserService _userService;
    public DeleteConsumptionReadingCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<bool> Consume(DeleteConsumptionReadingCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);
        var entity = await session.Query<ConsumptionReading>()
            .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct).ConfigureAwait(false);
        if (entity == null) return false;
        entity.IsDeleted = true;
        entity.TraceUpdate(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return true;
    }
}
