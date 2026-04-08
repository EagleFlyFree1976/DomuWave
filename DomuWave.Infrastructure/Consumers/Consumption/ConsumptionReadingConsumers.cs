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

        var meterIds    = command.Items.Select(i => i.MeterId).Distinct().ToList();
        var existingIds = command.Items.Where(i => i.Id > 0).Select(i => i.Id).ToList();

        var meterById = (await session.Query<Meter>()
            .Where(m => meterIds.Contains(m.Id) && !m.IsDeleted)
            .ToListAsync(ct).ConfigureAwait(false))
            .ToDictionary(m => m.Id);

        var existingById = existingIds.Count > 0
            ? (await session.Query<ConsumptionReading>()
                .Where(r => existingIds.Contains(r.Id) && !r.IsDeleted)
                .ToListAsync(ct).ConfigureAwait(false))
                .ToDictionary(r => r.Id)
            : new Dictionary<int, ConsumptionReading>();

        // Cerca la ripartizione Draft attiva per questo tipo consumo + esercizio
        // (le nuove letture vengono automaticamente assegnate ad essa)
        var draftCharge = await session.Query<ConsumptionCharge>()
            .FirstOrDefaultAsync(c => c.ConsumptionType.Id == consumptionType.Id
                                   && c.FiscalYear.Id == fiscalYear.Id
                                   && c.Status.Id == ConsumptionChargeStatus.Draft
                                   && !c.IsDeleted, ct).ConfigureAwait(false);

        var result = new List<ConsumptionReading>();

        foreach (var item in command.Items)
        {
            if (!meterById.TryGetValue(item.MeterId, out var meter)) continue;

            ConsumptionReading reading;
            if (item.Id > 0 && existingById.TryGetValue(item.Id, out var existing))
            {
                // Lettura esistente: modificabile solo se non ha una ripartizione o ce l'ha in Draft
                if (existing.Charge != null && existing.Charge.Status?.Id != ConsumptionChargeStatus.Draft)
                    continue; // approvata — salta silenziosamente

                existing.InitialDate  = item.InitialDate;
                existing.InitialValue = item.InitialValue;
                existing.FinalDate    = item.FinalDate;
                existing.FinalValue   = item.FinalValue;
                existing.Notes        = item.Notes;
                existing.Trace(currentUser);
                reading = existing;
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
                    Charge       = draftCharge,   // assegna subito alla Draft se esiste
                    IsDeleted    = false,
                };
                reading.Trace(currentUser);
            }

            await session.SaveOrUpdateAsync(reading, ct).ConfigureAwait(false);
            result.Add(reading);
        }

        await session.FlushAsync(ct).ConfigureAwait(false);

        // Rilegge per avere Consumption calcolato dal DB
        foreach (var r in result)
            await session.RefreshAsync(r, ct).ConfigureAwait(false);

        return result.Select(x => x.ToReadDto()).ToList();
    }
}

public class GetUnchargedReadingsCountByFiscalYearCommandConsumer
    : InMemoryConsumerBase<GetUnchargedReadingsCountByFiscalYearCommand, int>
{
    private readonly IUserService _userService;
    public GetUnchargedReadingsCountByFiscalYearCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<int> Consume(GetUnchargedReadingsCountByFiscalYearCommand command, IMediationContext ctx, CancellationToken ct)
        => await session.Query<ConsumptionReading>()
            .CountAsync(r => r.FiscalYear.Id == command.FiscalYearId
                          && r.Charge == null
                          && !r.IsDeleted, ct).ConfigureAwait(false);
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
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return true;
    }
}
