using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.BudgetItem;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateBudgetItemCommandConsumer
    : InMemoryConsumerBase<CreateBudgetItemCommand, BudgetItemReadDto>
{
    private readonly IBudgetItemService        _budgetItemService;
    private readonly IBudgetService            _budgetService;
    private readonly IChartOfAccountsService   _accountService;
    private readonly IUserService              _userService;

    public CreateBudgetItemCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IBudgetItemService budgetItemService,
        IBudgetService budgetService,
        IChartOfAccountsService accountService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _budgetItemService = budgetItemService;
        _budgetService     = budgetService;
        _accountService    = accountService;
        _userService       = userService;
    }

    protected override async Task<BudgetItemReadDto> Consume(
        CreateBudgetItemCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var budget = await _budgetService
            .GetByIdAsync(command.Dto.BudgetId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (budget == null)
            throw new NotFoundException("Budget non trovato.");
        if (budget.Status.Id != BudgetStatus.Draft)
            throw new ValidatorException("È possibile aggiungere voci solo a un budget in bozza.");

        var account = await _accountService
            .GetByIdAsync(command.Dto.AccountId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (account == null)
            throw new NotFoundException("Conto non trovato.");

        var item = new BudgetItem
        {
            Budget      = budget,
            Tenant      = budget.Tenant,
            Account     = account,
            AccountCode = account.Code,
            AccountName = account.Name,
            Name        = command.Dto.Description ?? string.Empty,
            Amount      = command.Dto.Amount,
            Notes       = command.Dto.Notes,
        };

        var created = await _budgetItemService
            .CreateAsync(item, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // ── Assicura che tutti gli antenati del conto siano presenti nel budget ──
        // Risale la catena dei parent navigando il proxy NHibernate già in sessione.
        var ancestors = new List<ChartOfAccounts>();
        var cursor = account.ParentAccount;
        while (cursor != null)
        {
            // Forza il caricamento dell'entità per evitare proxy non inizializzati
            cursor = await session.GetAsync<ChartOfAccounts>(cursor.Id, cancellationToken)
                .ConfigureAwait(false);
            if (cursor == null) break;
            ancestors.Add(cursor);
            cursor = cursor.ParentAccount;
        }

        if (ancestors.Count > 0)
        {
            // Carica gli item già presenti nel budget per questi account
            var ancestorIds = ancestors.Select(a => a.Id).ToList();
            var existingAncestorAccountIds = await session.Query<BudgetItem>()
                .Where(x => x.Budget.Id == budget.Id
                         && ancestorIds.Contains(x.Account.Id)
                         && !x.IsDeleted)
                .Select(x => x.Account.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            var existingSet = new HashSet<int>(existingAncestorAccountIds);

            foreach (var ancestor in ancestors)
            {
                if (existingSet.Contains(ancestor.Id)) continue;

                var ancestorItem = new BudgetItem
                {
                    Budget      = budget,
                    Tenant      = budget.Tenant,
                    Account     = ancestor,
                    AccountCode = ancestor.Code,
                    AccountName = ancestor.Name,
                    Name        = string.Empty,
                    Amount      = 0,
                };
                ancestorItem.Trace(currentUser);
                await session.SaveAsync(ancestorItem, cancellationToken).ConfigureAwait(false);
            }

            await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        // Ricalcola totali del budget in base al tipo conto
        var allItems = await _budgetItemService
            .GetByBudgetIdAsync(budget.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
        var parentAccountIds = allItems.Where(i => i.Account?.ParentAccount != null)
            .Select(i => i.Account.ParentAccount.Id).ToHashSet();
        var leafItems = allItems.Where(i => i.Account != null && !parentAccountIds.Contains(i.Account.Id)).ToList();
        budget.TotalExpenses = leafItems.Where(i => i.Account.Type == ChartOfAccountsType.Uscita).Sum(i => i.Amount);
        budget.TotalIncome   = leafItems.Where(i => i.Account.Type != ChartOfAccountsType.Uscita).Sum(i => i.Amount);
        await _budgetService.UpdateAsync(budget, currentUser, cancellationToken).ConfigureAwait(false);

        return created.ToReadDto();
    }
}
