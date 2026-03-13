using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.AccountBalance;
using DomuWave.Services.Dto.Contabilita.AccountBalance;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateAccountBalanceOpeningCommandConsumer
    : InMemoryConsumerBase<UpdateAccountBalanceOpeningCommand, AccountBalanceReadDto>
{
    private readonly IUserService _userService;

    public UpdateAccountBalanceOpeningCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<AccountBalanceReadDto> Consume(
        UpdateAccountBalanceOpeningCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var balance = await session.Query<AccountBalance>()
            .FirstOrDefaultAsync(b => b.Id == command.Id && !b.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (balance == null)
            throw new NotFoundException("Saldo non trovato.");

        var fiscalYear = balance.FiscalYear;

        // Verifico che l'esercizio non sia chiuso o bloccato
        if (fiscalYear.Status?.Id == FiscalYearStatus.Closed || fiscalYear.Status?.Id == FiscalYearStatus.Locked)
            throw new ValidatorException("Impossibile modificare il saldo iniziale: l'esercizio è già chiuso.");

        // Verifico che sia il PRIMO esercizio del condominio
        var isFirstFiscalYear = !await session.Query<FiscalYear>()
            .AnyAsync(f => f.Condominium.Id == fiscalYear.Condominium.Id
                        && f.Id != fiscalYear.Id
                        && f.StartDate < fiscalYear.StartDate
                        && !f.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (!isFirstFiscalYear)
            throw new ValidatorException(
                "Il saldo iniziale è modificabile manualmente solo per il primo esercizio del condominio. " +
                "Per gli esercizi successivi viene ereditato automaticamente dal saldo finale di quello precedente.");

        balance.OpeningBalance = command.Dto.OpeningBalance;
        balance.Trace(currentUser);
        await session.UpdateAsync(balance, cancellationToken).ConfigureAwait(false);

        // Movimenti per il conto nell'esercizio
        var movTotal = await session.Query<Expense>()
            .Where(e => e.FiscalYear.Id == fiscalYear.Id && e.Account.Id == balance.Account.Id && !e.IsDeleted)
            .SumAsync(e => (decimal?)e.NetAmount, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        var signedMov = balance.Account.Type == ChartOfAccountsType.Uscita ? -movTotal : movTotal;

        // Arrotondamenti aggregati per questo conto nell'esercizio
        var totalRounding = await session.Query<ExpenseAllocation>()
            .Where(ea => ea.Expense.FiscalYear.Id == fiscalYear.Id
                      && ea.Expense.Account.Id == balance.Account.Id
                      && !ea.IsDeleted)
            .SumAsync(ea => (decimal?)ea.RoundingAdjustment, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        return new AccountBalanceReadDto
        {
            Id                       = balance.Id,
            FiscalYearId             = fiscalYear.Id,
            AccountId                = balance.Account.Id,
            AccountCode              = balance.Account.Code ?? string.Empty,
            AccountName              = balance.Account.Name ?? string.Empty,
            AccountType              = (int)balance.Account.Type,
            AccountTypeLabel         = balance.Account.Type switch
            {
                ChartOfAccountsType.Entrata      => "Entrata",
                ChartOfAccountsType.Patrimoniale => "Patrimoniale",
                _                                => "Uscita",
            },
            OpeningBalance           = balance.OpeningBalance,
            MovementsTotal           = movTotal,
            TotalBalance             = balance.OpeningBalance + signedMov,
            ClosingBalance           = balance.ClosingBalance,
            IsOpeningBalanceEditable = true,
            TotalRoundingAdjustment  = totalRounding,
        };
    }
}
