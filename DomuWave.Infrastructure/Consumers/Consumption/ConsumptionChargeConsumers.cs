using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Consumption;
using DomuWave.Services.Dto.Consumption;
using DomuWave.Services.Helpers;
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
            .OrderBy(x => x.Status.Id)   // Draft (1) prima di Approved (2)
            .ThenBy(x => x.ConsumptionType.Name)
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

        // Recupera automaticamente il budget preventivo dell'esercizio
        var budget = await session.Query<Budget>()
            .FirstOrDefaultAsync(x => x.FiscalYear.Id == command.Dto.FiscalYearId
                                   && x.Type == BudgetType.Preventivo
                                   && !x.IsDeleted, ct).ConfigureAwait(false);
        if (budget == null) throw new ValidatorException(
            "Non esiste un budget preventivo per questo esercizio. Crea prima il preventivo.");

        Expense? expense = null;
        if (command.Dto.ExpenseId.HasValue)
        {
            expense = await session.Query<Expense>()
                .FirstOrDefaultAsync(x => x.Id == command.Dto.ExpenseId.Value && !x.IsDeleted, ct).ConfigureAwait(false);
        }

        // Blocca se il budget consuntivo dell'esercizio è già approvato o chiuso
        var consuntivoApproved = await session.Query<Budget>()
            .AnyAsync(b => b.FiscalYear.Id == command.Dto.FiscalYearId
                        && b.Type          == BudgetType.Consuntivo
                        && (b.Status.Id == BudgetStatus.Approved || b.Status.Id == BudgetStatus.Closed)
                        && !b.IsDeleted, ct).ConfigureAwait(false);
        if (consuntivoApproved)
            throw new ValidatorException(
                "Non è possibile creare una ripartizione consumi: il budget consuntivo di questo esercizio è già stato approvato. " +
                "Riapri il consuntivo oppure seleziona un altro esercizio.");

        // Blocca se esiste già una ripartizione in Draft per questo tipo+esercizio
        var existingDraft = await session.Query<ConsumptionCharge>()
            .AnyAsync(x => x.ConsumptionType.Id == command.Dto.ConsumptionTypeId
                        && x.FiscalYear.Id == command.Dto.FiscalYearId
                        && x.Status.Id == ConsumptionChargeStatus.Draft
                        && !x.IsDeleted, ct).ConfigureAwait(false);
        if (existingDraft)
            throw new ValidatorException("Esiste già una ripartizione in bozza per questo tipo di consumo. Approva o elimina quella esistente prima di crearne una nuova.");

        var draftStatus = await session.Query<ConsumptionChargeStatus>()
            .FirstOrDefaultAsync(x => x.Id == ConsumptionChargeStatus.Draft, ct).ConfigureAwait(false);

        var entity = new ConsumptionCharge
        {
            ConsumptionType = consumptionType,
            FiscalYear      = fiscalYear,
            Budget          = budget,
            Expense         = expense,
            TotalAmount     = 0m,   // calcolato in RecalculateItems come somma delle bollette
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

        if (command.Dto.ExpenseId.HasValue)
        {
            entity.Expense = await session.Query<Expense>()
                .FirstOrDefaultAsync(x => x.Id == command.Dto.ExpenseId.Value && !x.IsDeleted, ct).ConfigureAwait(false);
        }
        else
        {
            entity.Expense = null;
        }

        // TotalAmount è ricalcolato come somma delle bollette in RecalculateItems
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
/// Salva gli importi modificati manualmente sulle quote. Marca la ripartizione come
/// manuale (IsManual = true) così che i ricalcoli successivi non sovrascrivano gli importi.
/// Vincolo: la somma degli importi deve essere uguale al TotalAmount (somma delle bollette).
/// </summary>
public class SaveManualConsumptionChargeItemsCommandConsumer
    : InMemoryConsumerBase<SaveManualConsumptionChargeItemsCommand, ConsumptionChargeReadDto>
{
    private readonly IUserService _userService;
    public SaveManualConsumptionChargeItemsCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<ConsumptionChargeReadDto> Consume(SaveManualConsumptionChargeItemsCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);

        var entity = await session.Query<ConsumptionCharge>()
            .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct).ConfigureAwait(false);
        if (entity == null) throw new NotFoundException("Ripartizione non trovata.");
        if (entity.Status?.Id == ConsumptionChargeStatus.Approved)
            throw new ValidatorException("Non è possibile modificare una ripartizione già approvata.");

        var inputItems = command.Dto?.Items ?? [];
        if (!inputItems.Any())
            throw new ValidatorException("Nessun importo da salvare.");

        // Importi negativi non ammessi
        if (inputItems.Any(i => i.Amount < 0))
            throw new ValidatorException("Gli importi non possono essere negativi.");

        // ── Vincolo: somma importi == TotalAmount (somma bollette) ─────────────
        var sum = inputItems.Sum(i => Math.Round(i.Amount, 2));
        var total = Math.Round(entity.TotalAmount, 2);
        if (sum != total)
        {
            var diff = total - sum;
            throw new ValidatorException(
                $"La somma degli importi ({sum:N2}) deve essere uguale al totale delle bollette ({total:N2}). " +
                $"Differenza: {diff:N2}.");
        }

        var existingItems = await session.Query<ConsumptionChargeItem>()
            .Where(i => i.Charge.Id == entity.Id && !i.IsDeleted)
            .ToListAsync(ct).ConfigureAwait(false);
        var existingByUnit = existingItems.ToDictionary(i => i.Unit.Id);

        // Gli importi manuali devono riferirsi a quote esistenti della ripartizione
        var unknown = inputItems.Where(i => !existingByUnit.ContainsKey(i.UnitId)).ToList();
        if (unknown.Any())
            throw new ValidatorException("Alcune unità indicate non fanno parte di questa ripartizione. Ricalcola e riprova.");

        foreach (var input in inputItems)
        {
            var item   = existingByUnit[input.UnitId];
            var amount = Math.Round(input.Amount, 2);
            item.Amount = amount;
            // Allinea la percentuale all'importo manuale: è usata in fase di approvazione
            // per ripartire le singole bollette (ExpenseAllocationHelper).
            item.Percentage = total > 0 ? amount / total : 0m;
            item.HasWarning = false;
            item.Trace(currentUser);
            await session.SaveOrUpdateAsync(item, ct).ConfigureAwait(false);
        }

        entity.IsManual = true;
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        await session.RefreshAsync(entity, ct).ConfigureAwait(false);

        return entity.ToReadDto();
    }
}

/// <summary>
/// Ripristina la ripartizione automatica: azzera il flag manuale e ricalcola gli importi
/// dai consumi registrati.
/// </summary>
public class ResetConsumptionChargeToAutoCommandConsumer
    : InMemoryConsumerBase<ResetConsumptionChargeToAutoCommand, ConsumptionChargeReadDto>
{
    private readonly IUserService _userService;
    public ResetConsumptionChargeToAutoCommandConsumer(ISessionFactoryProvider sp, IUserService us) : base(sp) => _userService = us;

    protected override async Task<ConsumptionChargeReadDto> Consume(ResetConsumptionChargeToAutoCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);

        var entity = await session.Query<ConsumptionCharge>()
            .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct).ConfigureAwait(false);
        if (entity == null) throw new NotFoundException("Ripartizione non trovata.");
        if (entity.Status?.Id == ConsumptionChargeStatus.Approved)
            throw new ValidatorException("Non è possibile modificare una ripartizione già approvata.");

        entity.IsManual = false;
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);

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

        // Blocca se il budget consuntivo dell'esercizio è già approvato o chiuso
        var consuntivoApproved = await session.Query<Budget>()
            .AnyAsync(b => b.FiscalYear.Id == entity.FiscalYear.Id
                        && b.Type          == BudgetType.Consuntivo
                        && (b.Status.Id == BudgetStatus.Approved || b.Status.Id == BudgetStatus.Closed)
                        && !b.IsDeleted, ct).ConfigureAwait(false);
        if (consuntivoApproved)
            throw new ValidatorException(
                "Non è possibile approvare la ripartizione: il budget consuntivo di questo esercizio è già stato approvato. " +
                "Riapri il consuntivo oppure sposta la ripartizione su un altro esercizio.");

        // Ricalcola prima di approvare: garantisce che l'importo (somma bollette) e i
        // consumi siano aggiornati e che TUTTE le unità abbiano un consumo (altrimenti
        // RecalculateItems lancia ValidatorException con l'elenco delle mancanti).
        await ConsumptionChargeHelper.RecalculateItems(entity, currentUser, ct, session).ConfigureAwait(false);

        if (entity.TotalAmount <= 0)
            throw new ValidatorException(
                "Non ci sono spese (bollette) registrate sul conto di questo tipo di consumo per l'esercizio: niente da ripartire.");

        if (!entity.Items.Any(i => !i.IsDeleted && i.Amount > 0))
            throw new ValidatorException("La ripartizione non produce quote: verifica consumi e spese registrate.");

        // Ripartizione manuale: il ricalcolo ha potuto aggiornare il totale bollette senza
        // toccare gli importi manuali. Verifica che la somma quadri ancora col nuovo totale.
        if (entity.IsManual)
        {
            var manualSum = Math.Round(entity.Items.Where(i => !i.IsDeleted).Sum(i => i.Amount), 2);
            if (manualSum != Math.Round(entity.TotalAmount, 2))
                throw new ValidatorException(
                    $"Gli importi manuali ({manualSum:N2}) non corrispondono più al totale delle bollette ({entity.TotalAmount:N2}): " +
                    "le spese sono cambiate dopo la modifica manuale. Aggiorna gli importi o ripristina la ripartizione automatica prima di approvare.");
        }

        // ── Cambia stato → Approved ───────────────────────────────────────────
        // L'approvazione della ripartizione consumi NON genera rate né quote
        // (CondominiumFee): le quote per unità (ConsumptionChargeItem) sono già
        // salvate e confluiscono nel CONSUNTIVO tramite le allocazioni delle bollette
        // (rigenerate per consumo qui sotto). L'addebito effettivo avviene quando si
        // approva il budget consuntivo, che genera le rate.
        // Le bollette sono già registrate durante l'anno sul conto del tipo consumo:
        // non si crea alcuna Expense "riepilogo".
        var approvedStatus = await session.Query<ConsumptionChargeStatus>()
            .FirstOrDefaultAsync(x => x.Id == ConsumptionChargeStatus.Approved, ct).ConfigureAwait(false);
        entity.Status = approvedStatus;
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);

        // ── Rigenera le allocazioni delle bollette del conto secondo i consumi ──
        if (entity.ConsumptionType?.Account != null)
        {
            var bollette = await session.Query<Expense>()
                .Where(e => e.FiscalYear.Id == entity.FiscalYear.Id
                         && e.Account.Id    == entity.ConsumptionType.Account.Id
                         && !e.IsDeleted)
                .ToListAsync(ct).ConfigureAwait(false);

            foreach (var bolletta in bollette)
                await ExpenseAllocationHelper
                    .RegenerateAllocationsAsync(session, bolletta, currentUser, ct)
                    .ConfigureAwait(false);
        }

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

        // Sgancia le letture collegate prima di eliminare la ripartizione
        var linkedReadings = await session.Query<ConsumptionReading>()
            .Where(r => r.Charge.Id == entity.Id && !r.IsDeleted)
            .ToListAsync(ct).ConfigureAwait(false);
        foreach (var r in linkedReadings)
        {
            r.Charge = null;
            await session.SaveOrUpdateAsync(r, ct).ConfigureAwait(false);
        }

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
    /// L'importo da ripartire NON è inserito manualmente: è la somma delle spese (bollette)
    /// registrate durante l'anno sul conto del tipo di consumo per quell'esercizio.
    /// La ripartizione è consentita solo se TUTTE le unità attive hanno un consumo (non null).
    /// </summary>
    internal static async Task RecalculateItems(ConsumptionCharge entity,
        IUser currentUser,
        CancellationToken ct,
        ISession session)
    {
        var user = currentUser as CPQ.Core.Memberships.IUser;

        // Il tipo di consumo deve avere un conto associato: è quello su cui sono
        // registrate le bollette da ripartire.
        if (entity.ConsumptionType?.Account == null)
            throw new ValidatorException(
                "Il tipo di consumo non ha un conto del piano dei conti associato: " +
                "impossibile determinare le spese da ripartire. Configura il conto sul tipo di consumo.");

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

        // ── Importo da ripartire = somma delle bollette sul conto del tipo consumo ──
        // Come per le allocazioni, l'importo a carico dei condòmini è il totale fattura:
        // GrossAmount (già al netto della ritenuta) + ritenuta d'acconto.
        var accountId = entity.ConsumptionType.Account.Id;
        var totalAmount = await session.Query<Expense>()
            .Where(e => e.FiscalYear.Id == entity.FiscalYear.Id
                     && e.Account.Id    == accountId
                     && !e.IsDeleted)
            .SumAsync(e => (decimal?)(e.GrossAmount + e.WithholdingTax), ct)
            .ConfigureAwait(false) ?? 0m;

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

        // Unità DA RIPARTIRE = solo quelle con un contatore attivo del tipo di consumo.
        // Le unità senza contatore (es. box senza acqua) non rientrano nella ripartizione
        // e non bloccano il vincolo.
        var meteredUnitIds = meters
            .Select(m => m.Unit.Id)
            .Distinct()
            .ToList();

        var units = await session.Query<RealEstateUnit>()
            .Where(u => meteredUnitIds.Contains(u.Id)
                     && u.IsActive && !u.IsDeleted)
            .ToListAsync(ct).ConfigureAwait(false);

        // ── Vincolo: tutte le unità CON CONTATORE ATTIVO devono avere un consumo ────
        var missing = units
            .Where(u => !consumptionByUnit.ContainsKey(u.Id))
            .Select(u => u.DisplayName ?? u.Name ?? u.InternalNumber ?? $"Unità #{u.Id}")
            .ToList();
        if (missing.Any())
            throw new ValidatorException(
                "Impossibile ripartire: le seguenti unità (con contatore attivo) non hanno una lettura registrata per questo esercizio:\n- "
                + string.Join("\n- ", missing));

        var totalConsumption = consumptionByUnit.Values.Sum();

        // Storicizza sull'entità l'importo effettivamente ripartito (somma bollette)
        entity.TotalAmount = totalAmount;

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
            var consumption = consumptionByUnit[unit.Id];   // garantito presente dal vincolo sopra

            decimal amount;
            decimal percentage;

            if (totalConsumption > 0)
            {
                percentage = consumption / totalConsumption;
                amount = isLastUnit
                    ? totalAmount - allocated
                    : Math.Round(totalAmount * percentage, 2);
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
                // Ripartizione manuale: non sovrascrivere importo e percentuale (allineata
                // all'importo manuale e usata in fase di approvazione per ripartire le bollette).
                if (!entity.IsManual)
                {
                    existing.Percentage = percentage;
                    existing.Amount     = amount;
                }
                existing.HasWarning       = false;
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
                    // Una nuova unità su una ripartizione manuale parte con quota 0:
                    // l'utente la imposterà a mano (il vincolo somma è validato al salvataggio).
                    Amount           = entity.IsManual ? 0m : amount,
                    HasWarning       = false,
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

        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);

        // Aggiorna la collezione in memoria
        await session.RefreshAsync(entity, ct).ConfigureAwait(false);
    }
}
