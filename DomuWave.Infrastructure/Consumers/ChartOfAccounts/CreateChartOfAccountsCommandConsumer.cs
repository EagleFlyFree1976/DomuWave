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

public class CreateChartOfAccountsCommandConsumer
    : InMemoryConsumerBase<CreateChartOfAccountsCommand, ChartOfAccountsReadDto>
{
    private readonly IChartOfAccountsService _accountService;
    private readonly ICondominiumService     _condominiumService;
    private readonly IUserService            _userService;

    public CreateChartOfAccountsCommandConsumer(
        ISessionFactoryProvider  sessionFactoryProvider,
        IChartOfAccountsService  accountService,
        ICondominiumService      condominiumService,
        IUserService             userService) : base(sessionFactoryProvider)
    {
        _accountService     = accountService;
        _condominiumService = condominiumService;
        _userService        = userService;
    }

    protected override async Task<ChartOfAccountsReadDto> Consume(
        CreateChartOfAccountsCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var dto = command.Dto;

        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new ValidatorException("Il codice conto è obbligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidatorException("Il nome conto è obbligatorio.");
        // dto.Type is a non-nullable enum — always has a value

        var condominium = await _condominiumService
            .GetByIdAsync(dto.CondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (condominium == null)
            throw new NotFoundException("Condominio non trovato.");

        var codeExists = await session.Query<ChartOfAccounts>()
            .AnyAsync(x => x.Condominium.Id == dto.CondominiumId
                        && x.Code == dto.Code.Trim()
                        && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (codeExists)
            throw new ValidatorException($"Esiste già un conto con codice '{dto.Code}' per questo condominio.");

        ChartOfAccounts? parent = null;
        if (dto.ParentAccountId.HasValue)
        {
            parent = await _accountService
                .GetByIdAsync(dto.ParentAccountId.Value, currentUser, cancellationToken)
                .ConfigureAwait(false);
            if (parent == null)
                throw new NotFoundException("Conto padre non trovato.");
        }

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

        var chargeabilityType = session.Load<ChargeabilityType>(dto.ChargeabilityTypeId);

        var entity  = dto.ToEntity(condominium, chargeabilityType, parent, category, defaultMillesimalTable);
        var created = await _accountService
            .CreateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // ── Aggiunge una voce vuota in ogni budget in bozza del condominio ────
        // I budget approvati/chiusi non vengono toccati (la loro struttura è congelata).
        if (created.IsActive)
        {
            var draftBudgets = await session.Query<Budget>()
                .Where(b => b.Condominium.Id == dto.CondominiumId
                         && b.Status.Id      == BudgetStatus.Draft
                         && !b.IsDeleted)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var budget in draftBudgets)
            {
                var newItem = new BudgetItem
                {
                    Budget      = budget,
                    Tenant      = budget.Tenant,
                    Account     = created,
                    AccountCode = created.Code,
                    AccountName = created.Name,
                    Name        = string.Empty,
                    Amount      = 0,
                };
                newItem.Trace(currentUser);
                await session.SaveAsync(newItem, cancellationToken).ConfigureAwait(false);
            }

            if (draftBudgets.Any())
                await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        return created.ToReadDto();
    }
}
