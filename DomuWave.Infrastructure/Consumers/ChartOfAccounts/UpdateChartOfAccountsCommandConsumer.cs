using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ChartOfAccounts;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateChartOfAccountsCommandConsumer
    : InMemoryConsumerBase<UpdateChartOfAccountsCommand, ChartOfAccountsReadDto>
{
    private readonly IChartOfAccountsService _accountService;
    private readonly IUserService            _userService;

    public UpdateChartOfAccountsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IChartOfAccountsService accountService,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _accountService = accountService;
        _userService    = userService;
    }

    // Carica tutti i conti del condominio, poi riallinea ricorsivamente i livelli
    // dei discendenti dell'entità appena aggiornata.
    private async Task RealignDescendantLevelsAsync(
        ChartOfAccounts root,
        CPQ.Core.Memberships.IUser currentUser,
        CancellationToken cancellationToken)
    {
        var allAccounts = await session.Query<ChartOfAccounts>()
            .Where(x => x.Condominium.Id == root.Condominium.Id && !x.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Mappa id → figli diretti
        var childrenMap = allAccounts
            .Where(x => x.ParentAccount != null)
            .GroupBy(x => x.ParentAccount.Id)
            .ToDictionary(g => g.Key, g => g.ToList());

        // BFS a partire dai figli diretti di root
        var queue = new Queue<ChartOfAccounts>();
        if (childrenMap.TryGetValue(root.Id, out var directChildren))
            foreach (var c in directChildren) queue.Enqueue(c);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            var expectedLevel = node.ParentAccount!.Level + 1;
            if (node.Level != expectedLevel)
            {
                node.Level = expectedLevel;
                node.Trace(currentUser);
                await session.SaveOrUpdateAsync(node, cancellationToken).ConfigureAwait(false);
            }

            if (childrenMap.TryGetValue(node.Id, out var children))
                foreach (var c in children) queue.Enqueue(c);
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    protected override async Task<ChartOfAccountsReadDto> Consume(
        UpdateChartOfAccountsCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var entity = await _accountService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (entity == null)
            throw new NotFoundException("Conto non trovato.");

        var dto = command.Dto;

        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new ValidatorException("Il codice conto è obbligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidatorException("Il nome conto è obbligatorio.");

        var codeExists = await session.Query<ChartOfAccounts>()
            .AnyAsync(x => x.Condominium.Id == entity.Condominium.Id
                        && x.Code == dto.Code.Trim()
                        && x.Id   != command.Id
                        && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (codeExists)
            throw new ValidatorException($"Esiste già un conto con codice '{dto.Code}' per questo condominio.");

        ChartOfAccountsCategory? category = null;
        if (dto.CategoryId.HasValue)
        {
            category = await session.Query<ChartOfAccountsCategory>()
                .FirstOrDefaultAsync(x => x.Id == dto.CategoryId.Value && !x.IsDeleted, cancellationToken)
                .ConfigureAwait(false);
            if (category == null)
                throw new NotFoundException("Categoria non trovata.");
        }

        MillesimalTable? defaultMillesimalTable = null;
        if (dto.DefaultMillesimalTableId.HasValue)
        {
            defaultMillesimalTable = await session.Query<MillesimalTable>()
                .FirstOrDefaultAsync(x => x.Id == dto.DefaultMillesimalTableId.Value && !x.IsDeleted, cancellationToken)
                .ConfigureAwait(false);
            if (defaultMillesimalTable == null)
                throw new NotFoundException("Tabella millesimale non trovata.");
        }

        ChartOfAccounts? parentAccount = null;
        if (dto.ParentAccountId.HasValue)
        {
            parentAccount = await session.Query<ChartOfAccounts>()
                .FirstOrDefaultAsync(x => x.Id == dto.ParentAccountId.Value && !x.IsDeleted, cancellationToken)
                .ConfigureAwait(false);
            if (parentAccount == null)
                throw new NotFoundException("Conto padre non trovato.");
        }

        var chargeabilityType = session.Load<ChargeabilityType>(dto.ChargeabilityTypeId);

        var previousIsActive = entity.IsActive;
        var previousCode     = entity.Code;
        var previousName     = entity.Name;

        entity.ApplyUpdate(dto, chargeabilityType, category, defaultMillesimalTable, parentAccount);
        var updated = await _accountService
            .UpdateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // Riallinea il livello di tutti i discendenti se il parent è cambiato
        await RealignDescendantLevelsAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // ── Sincronizza i budget in bozza dello stesso condominio ─────────────
        await SyncDraftBudgetsAsync(entity, previousIsActive, previousCode, previousName, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return updated.ToReadDto();
    }

    /// <summary>
    /// Dopo ogni modifica al piano dei conti, allinea i budget in stato Bozza:
    /// - Se il codice o il nome è cambiato: aggiorna lo snapshot AccountCode/AccountName
    ///   nelle voci di budget in bozza (i budget approvati/chiusi rimangono invariati).
    /// - Se il conto è stato disattivato: soft-delete delle voci in bozza associate.
    /// - Se il conto è stato riattivato o era già attivo: assicura che esista una voce
    ///   (Amount = 0) in ogni budget in bozza che non la possiede ancora.
    /// </summary>
    private async Task SyncDraftBudgetsAsync(
        ChartOfAccounts account,
        bool previousIsActive,
        string previousCode,
        string previousName,
        CPQ.Core.Memberships.IUser currentUser,
        CancellationToken cancellationToken)
    {
        // Trova tutti i budget in bozza per questo condominio
        var draftBudgets = await session.Query<Budget>()
            .Where(b => b.Condominium.Id == account.Condominium.Id
                     && b.Status.Id      == BudgetStatus.Draft
                     && !b.IsDeleted)
            .Select(b => new { b.Id, TenantId = b.Tenant.Id })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!draftBudgets.Any()) return;

        var budgetIds = draftBudgets.Select(b => b.Id).ToList();

        if (!account.IsActive)
        {
            // Conto disattivato: soft-delete delle voci in bozza
            var itemsToDelete = await session.Query<BudgetItem>()
                .Where(x => budgetIds.Contains(x.Budget.Id)
                         && x.Account.Id == account.Id
                         && !x.IsDeleted)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var item in itemsToDelete)
            {
                item.IsDeleted = true;
                item.Trace(currentUser);
                await session.SaveOrUpdateAsync(item, cancellationToken).ConfigureAwait(false);
            }
        }
        else
        {
            // Conto attivo: aggiorna snapshot o aggiunge voce mancante per ogni budget in bozza
            foreach (var draft in draftBudgets)
            {
                var existing = await session.Query<BudgetItem>()
                    .FirstOrDefaultAsync(x => x.Budget.Id  == draft.Id
                                           && x.Account.Id == account.Id
                                           && !x.IsDeleted, cancellationToken)
                    .ConfigureAwait(false);

                if (existing != null)
                {
                    // Voce già presente: aggiorna snapshot se codice o nome sono cambiati
                    if (existing.AccountCode != account.Code || existing.AccountName != account.Name)
                    {
                        existing.AccountCode = account.Code;
                        existing.AccountName = account.Name;
                        existing.Trace(currentUser);
                        await session.SaveOrUpdateAsync(existing, cancellationToken).ConfigureAwait(false);
                    }
                }
                else
                {
                    // Voce mancante: aggiunge con Amount = 0
                    var budget = await session.GetAsync<Budget>(draft.Id, cancellationToken).ConfigureAwait(false);
                    var newItem = new BudgetItem
                    {
                        Budget      = budget,
                        Tenant      = budget.Tenant,
                        Account     = account,
                        AccountCode = account.Code,
                        AccountName = account.Name,
                        Name        = string.Empty,
                        Amount      = 0,
                    };
                    newItem.Trace(currentUser);
                    await session.SaveAsync(newItem, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}
