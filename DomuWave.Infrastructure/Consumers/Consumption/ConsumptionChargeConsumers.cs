using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Consumption;
using DomuWave.Services.Dto.Consumption;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetConsumptionChargesByFiscalYearCommandConsumer
    : InMemoryConsumerBase<GetConsumptionChargesByFiscalYearCommand, IList<ConsumptionChargeReadDto>>
{
    private readonly IUserService _userService;
    public GetConsumptionChargesByFiscalYearCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<IList<ConsumptionChargeReadDto>> Consume(GetConsumptionChargesByFiscalYearCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var items = await session.Query<ConsumptionCharge>()
            .Where(x => x.FiscalYear.Id == command.FiscalYearId && !x.IsDeleted)
            .OrderBy(x => x.ConsumptionType.Name)
            .ToListAsync(ct).ConfigureAwait(false);
        return items.Select(x => x.ToReadDto()).ToList();
    }
}

public class GetConsumptionChargeByIdCommandConsumer
    : InMemoryConsumerBase<GetConsumptionChargeByIdCommand, ConsumptionChargeReadDto>
{
    private readonly IUserService _userService;
    public GetConsumptionChargeByIdCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<ConsumptionChargeReadDto> Consume(GetConsumptionChargeByIdCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var item = await session.Query<ConsumptionCharge>()
            .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct).ConfigureAwait(false);
        return item?.ToReadDto();
    }
}

public class CreateConsumptionChargeCommandConsumer
    : InMemoryConsumerBase<CreateConsumptionChargeCommand, ConsumptionChargeReadDto>
{
    private readonly IUserService _userService;
    public CreateConsumptionChargeCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<ConsumptionChargeReadDto> Consume(CreateConsumptionChargeCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);

        var consumptionType = await session.Query<ConsumptionType>()
            .FirstOrDefaultAsync(x => x.Id == command.Dto.ConsumptionTypeId && !x.IsDeleted, ct).ConfigureAwait(false);
        if (consumptionType == null) throw new NotFoundException("Tipo consumo non trovato.");

        var fiscalYear = await session.Query<FiscalYear>()
            .FirstOrDefaultAsync(x => x.Id == command.Dto.FiscalYearId && !x.IsDeleted, ct).ConfigureAwait(false);
        if (fiscalYear == null) throw new NotFoundException("Esercizio non trovato.");

        var budget = await session.Query<Budget>()
            .FirstOrDefaultAsync(x => x.Id == command.Dto.BudgetId && !x.IsDeleted, ct).ConfigureAwait(false);
        if (budget == null) throw new NotFoundException("Budget non trovato.");

        Expense expense = null;
        if (command.Dto.ExpenseId.HasValue)
        {
            expense = await session.Query<Expense>()
                .FirstOrDefaultAsync(x => x.Id == command.Dto.ExpenseId.Value && !x.IsDeleted, ct).ConfigureAwait(false);
        }

        // Blocca se esiste già una ripartizione in Draft per questo tipo+esercizio
        var existingDraft = await session.Query<ConsumptionCharge>()
            .AnyAsync(x => x.ConsumptionType.Id == command.Dto.ConsumptionTypeId
                        && x.FiscalYear.Id == command.Dto.FiscalYearId
                        && x.Status.Id == ConsumptionChargeStatus.Draft
                        && !x.IsDeleted, ct).ConfigureAwait(false);
        if (existingDraft)
            throw new ValidatorException("Esiste già una ripartizione in bozza per questo tipo di consumo. Approva o elimina quella esistente prima di crearne una nuova.");

        // Verifica che esistano letture non ancora ripartite per questo tipo+esercizio
        var meterIdsForType = await session.Query<Meter>()
            .Where(m => m.ConsumptionType.Id == command.Dto.ConsumptionTypeId && m.IsActive && !m.IsDeleted)
            .Select(m => m.Id)
            .ToListAsync(ct).ConfigureAwait(false);

        var hasUnchargedReadings = await session.Query<ConsumptionReading>()
            .AnyAsync(r => meterIdsForType.Contains(r.Meter.Id)
                        && r.FiscalYear.Id == command.Dto.FiscalYearId
                        && r.Charge == null
                        && !r.IsDeleted, ct).ConfigureAwait(false);

        if (!hasUnchargedReadings)
            throw new ValidatorException("Non esistono letture non ancora ripartite per questo tipo di consumo nell'esercizio selezionato.");

        var draftStatus = await session.Query<ConsumptionChargeStatus>()
            .FirstOrDefaultAsync(x => x.Id == ConsumptionChargeStatus.Draft, ct).ConfigureAwait(false);

        var entity = new ConsumptionCharge
        {
            ConsumptionType = consumptionType,
            FiscalYear      = fiscalYear,
            Budget          = budget,
            Expense         = expense,
            TotalAmount     = command.Dto.TotalAmount,
            Status          = draftStatus,
            Notes           = command.Dto.Notes,
            Tenant          = consumptionType.Tenant,
            IsDeleted       = false,
        };
        entity.Trace(currentUser);
        await session.SaveAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);

        // Calcola subito la ripartizione
        await ConsumptionChargeHelper.RecalculateItems(entity, currentUser, ct, session).ConfigureAwait(false);

        return entity.ToReadDto();
    }
}

public class UpdateConsumptionChargeCommandConsumer
    : InMemoryConsumerBase<UpdateConsumptionChargeCommand, ConsumptionChargeReadDto>
{
    private readonly IUserService _userService;
    public UpdateConsumptionChargeCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<ConsumptionChargeReadDto> Consume(UpdateConsumptionChargeCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);

        var entity = await session.Query<ConsumptionCharge>()
            .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct).ConfigureAwait(false);
        if (entity == null) throw new NotFoundException("Ripartizione non trovata.");
        if (entity.Status?.Id == ConsumptionChargeStatus.Approved)
            throw new ValidatorException("Non è possibile modificare una ripartizione già approvata.");

        if (command.Dto.BudgetId.HasValue)
        {
            var budget = await session.Query<Budget>()
                .FirstOrDefaultAsync(x => x.Id == command.Dto.BudgetId.Value && !x.IsDeleted, ct).ConfigureAwait(false);
            if (budget == null) throw new NotFoundException("Budget non trovato.");
            entity.Budget = budget;
        }

        if (command.Dto.ExpenseId.HasValue)
        {
            entity.Expense = await session.Query<Expense>()
                .FirstOrDefaultAsync(x => x.Id == command.Dto.ExpenseId.Value && !x.IsDeleted, ct).ConfigureAwait(false);
        }
        else
        {
            entity.Expense = null;
        }

        entity.TotalAmount = command.Dto.TotalAmount;
        entity.Notes       = command.Dto.Notes;
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);

        // Ricalcola la ripartizione con il nuovo importo
        await ConsumptionChargeHelper.RecalculateItems(entity, currentUser, ct, session).ConfigureAwait(false);

        return entity.ToReadDto();
    }
}

public class RecalculateConsumptionChargeCommandConsumer
    : InMemoryConsumerBase<RecalculateConsumptionChargeCommand, ConsumptionChargeReadDto>
{
    private readonly IUserService _userService;
    public RecalculateConsumptionChargeCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<ConsumptionChargeReadDto> Consume(RecalculateConsumptionChargeCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);

        var entity = await session.Query<ConsumptionCharge>()
            .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct).ConfigureAwait(false);
        if (entity == null) throw new NotFoundException("Ripartizione non trovata.");
        if (entity.Status?.Id == ConsumptionChargeStatus.Approved)
            throw new ValidatorException("Non è possibile ricalcolare una ripartizione già approvata.");

        await ConsumptionChargeHelper.RecalculateItems(entity, currentUser, ct, session).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}

/// <summary>
/// Approva la ripartizione: genera una CondominiumFee per ogni unità
/// sull'installment del budget preventivo indicato.
/// </summary>
public class ApproveConsumptionChargeCommandConsumer
    : InMemoryConsumerBase<ApproveConsumptionChargeCommand, ConsumptionChargeReadDto>
{
    private readonly IUserService _userService;
    public ApproveConsumptionChargeCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<ConsumptionChargeReadDto> Consume(ApproveConsumptionChargeCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);
        var user = currentUser as CPQ.Core.Memberships.IUser;

        var entity = await session.Query<ConsumptionCharge>()
            .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct).ConfigureAwait(false);
        if (entity == null) throw new NotFoundException("Ripartizione non trovata.");
        if (entity.Status?.Id == ConsumptionChargeStatus.Approved)
            throw new ValidatorException("La ripartizione è già approvata.");
        if (!entity.Items.Any(i => !i.IsDeleted))
            throw new ValidatorException("Calcola prima la ripartizione prima di approvarla.");

        // Recupera l'installment aperto del budget (il più recente)
        var installment = await session.Query<CondominiumInstallment>()
            .Where(x => x.Budget.Id == entity.Budget.Id
                     && x.Status.Id == CondominiumInstallmentStatus.Open
                     && !x.IsDeleted)
            .OrderByDescending(x => x.DueDate)
            .FirstOrDefaultAsync(ct).ConfigureAwait(false);

        if (installment == null)
            throw new ValidatorException(
                "Non esiste una rata aperta nel budget selezionato su cui addebitare i consumi. " +
                "Verifica che il budget preventivo abbia rate generate e ancora aperte.");

        var openStatus = await session.Query<CondominiumInstallmentStatus>()
            .FirstOrDefaultAsync(x => x.Id == CondominiumInstallmentStatus.Open, ct).ConfigureAwait(false);

        foreach (var item in entity.Items.Where(i => !i.IsDeleted && i.Amount > 0))
        {
            var owner = await session.Query<UnitOwner>()
                .FirstOrDefaultAsync(x => x.Unit.Id == item.Unit.Id && x.IsActive && !x.IsDeleted, ct)
                .ConfigureAwait(false);

            var fee = new CondominiumFee
            {
                Installment   = installment,
                Unit          = item.Unit,
                UserId        = owner?.UserId ?? 0,
                Tenant        = entity.Tenant,
                AmountDue     = item.Amount,
                AmountPaid    = 0m,
                Balance       = item.Amount,
                PaymentStatus = "ToPay",
                Notes         = $"Consumo {entity.ConsumptionType?.Name} – {entity.FiscalYear?.Code}",
            };
            if (user != null) fee.Trace(user);
            await session.SaveAsync(fee, ct).ConfigureAwait(false);
        }

        // Cambia stato → Approved
        var approvedStatus = await session.Query<ConsumptionChargeStatus>()
            .FirstOrDefaultAsync(x => x.Id == ConsumptionChargeStatus.Approved, ct).ConfigureAwait(false);
        entity.Status = approvedStatus;
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);

        return entity.ToReadDto();
    }
}

public class DeleteConsumptionChargeCommandConsumer
    : InMemoryConsumerBase<DeleteConsumptionChargeCommand, bool>
{
    private readonly IUserService _userService;
    public DeleteConsumptionChargeCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<bool> Consume(DeleteConsumptionChargeCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);
        var entity = await session.Query<ConsumptionCharge>()
            .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct).ConfigureAwait(false);
        if (entity == null) return false;
        if (entity.Status?.Id == ConsumptionChargeStatus.Approved)
            throw new ValidatorException("Non è possibile eliminare una ripartizione già approvata.");
        entity.IsDeleted = true;
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return true;
    }
}

// ── Helper condiviso ──────────────────────────────────────────────────────

internal static class ConsumptionChargeHelper
{
    /// <summary>
    /// Calcola o ricalcola gli item della ripartizione in base ai consumi registrati.
    /// Logica: proporzionale al consumo totale di tutte le unità del tipo di consumo.
    /// Le unità senza letture ricevono quota 0 con HasWarning=true.
    /// </summary>
    internal static async Task RecalculateItems(ConsumptionCharge entity,
        IUser currentUser,
        CancellationToken ct,
        ISession session)
    {
        var user = currentUser as CPQ.Core.Memberships.IUser;

        // De-marca le letture precedentemente assegnate a questa ripartizione
        // (vengono rimesse nel pool "non ancora ripartite")
        var previousReadings = await session.Query<ConsumptionReading>()
            .Where(r => r.Charge.Id == entity.Id && !r.IsDeleted)
            .ToListAsync(ct).ConfigureAwait(false);
        foreach (var r in previousReadings)
        {
            r.Charge = null;
            await session.SaveOrUpdateAsync(r, ct).ConfigureAwait(false);
        }

        // Tutti i contatori attivi del tipo di consumo
        var meters = await session.Query<Meter>()
            .Where(m => m.ConsumptionType.Id == entity.ConsumptionType.Id
                     && m.IsActive && !m.IsDeleted)
            .ToListAsync(ct).ConfigureAwait(false);

        // Letture non ancora ripartite per l'esercizio
        var meterIds = meters.Select(m => m.Id).ToList();
        var readings = await session.Query<ConsumptionReading>()
            .Where(r => meterIds.Contains(r.Meter.Id)
                     && r.FiscalYear.Id == entity.FiscalYear.Id
                     && r.Charge == null
                     && !r.IsDeleted)
            .ToListAsync(ct).ConfigureAwait(false);

        // Aggrega per unità (somma letture dello stesso tipo sulla stessa unità)
        var consumptionByUnit = readings
            .GroupBy(r => r.Meter.Unit.Id)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Consumption));

        // Unità attive del condominio
        var units = await session.Query<RealEstateUnit>()
            .Where(u => u.Condominium.Id == entity.ConsumptionType.Condominium.Id
                     && u.IsActive && !u.IsDeleted)
            .ToListAsync(ct).ConfigureAwait(false);

        var totalConsumption = consumptionByUnit.Values.Sum();

        // Soft-delete degli item esistenti per ricrearli
        var existingItems = await session.Query<ConsumptionChargeItem>()
            .Where(i => i.Charge.Id == entity.Id && !i.IsDeleted)
            .ToListAsync(ct).ConfigureAwait(false);

        var existingByUnit = existingItems.ToDictionary(i => i.Unit.Id);

        var allocated = 0m;
        var unitList  = units.ToList();

        for (int idx = 0; idx < unitList.Count; idx++)
        {
            var unit       = unitList[idx];
            var isLastUnit = idx == unitList.Count - 1;
            var consumption = consumptionByUnit.TryGetValue(unit.Id, out var c) ? c : 0m;
            var hasWarning  = !consumptionByUnit.ContainsKey(unit.Id);

            decimal amount;
            decimal percentage;

            if (totalConsumption > 0 && !hasWarning)
            {
                percentage = consumption / totalConsumption;
                amount = isLastUnit
                    ? entity.TotalAmount - allocated
                    : Math.Round(entity.TotalAmount * percentage, 2);
                if (!isLastUnit) allocated += amount;
            }
            else
            {
                percentage = 0m;
                amount     = 0m;
            }

            if (existingByUnit.TryGetValue(unit.Id, out var existing))
            {
                existing.Consumption      = consumption;
                existing.TotalConsumption = totalConsumption;
                existing.Percentage       = percentage;
                existing.Amount           = amount;
                existing.HasWarning       = hasWarning;
                existing.Trace(currentUser);
                await session.SaveOrUpdateAsync(existing, ct).ConfigureAwait(false);
            }
            else
            {
                var item = new ConsumptionChargeItem
                {
                    Charge           = entity,
                    Unit             = unit,
                    Tenant           = entity.Tenant,
                    Consumption      = consumption,
                    TotalConsumption = totalConsumption,
                    Percentage       = percentage,
                    Amount           = amount,
                    HasWarning       = hasWarning,
                    IsDeleted        = false,
                };
                if (user != null) item.Trace(user);
                await session.SaveAsync(item, ct).ConfigureAwait(false);
            }
        }

        // Marca le letture usate come appartenenti a questa ripartizione
        foreach (var r in readings)
        {
            r.Charge = entity;
            await session.SaveOrUpdateAsync(r, ct).ConfigureAwait(false);
        }

        await session.FlushAsync(ct).ConfigureAwait(false);

        // Aggiorna la collezione in memoria
        await session.RefreshAsync(entity, ct).ConfigureAwait(false);
    }
}
