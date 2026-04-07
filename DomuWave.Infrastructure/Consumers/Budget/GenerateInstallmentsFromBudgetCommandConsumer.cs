using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Budget;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GenerateInstallmentsFromBudgetCommandConsumer
    : InMemoryConsumerBase<GenerateInstallmentsFromBudgetCommand, bool>
{
    private readonly IUserService _userService;

    public GenerateInstallmentsFromBudgetCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<bool> Consume(
        GenerateInstallmentsFromBudgetCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var budget = await session.Query<Budget>()
            .FirstOrDefaultAsync(x => x.Id == command.BudgetId && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (budget == null)
            throw new NotFoundException("Budget non trovato.");

        if (budget.Status?.Id != BudgetStatus.Approved && budget.Status?.Id != BudgetStatus.Closed)
            throw new ValidatorException("Le rate possono essere generate solo da un budget approvato o chiuso.");

        if (budget.Status?.Id == BudgetStatus.Closed)
            throw new ValidatorException("Non è possibile rigenerare le rate di un budget chiuso.");

        // Elimina le rate (e quote) esistenti se force = true, altrimenti blocca
        var existingInstallments = await session.Query<CondominiumInstallment>()
            .Where(x => x.Budget.Id == budget.Id && !x.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (existingInstallments.Count > 0)
        {
            if (!command.Force)
                throw new ValidatorException("Le rate per questo budget sono già state generate.");

            // Soft-delete quote e rate esistenti
            foreach (var inst in existingInstallments)
            {
                var fees = await session.Query<CondominiumFee>()
                    .Where(f => f.Installment.Id == inst.Id && !f.IsDeleted)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                foreach (var fee in fees)
                {
                    fee.IsDeleted = true;
                    await session.SaveOrUpdateAsync(fee, cancellationToken).ConfigureAwait(false);
                }

                inst.IsDeleted = true;
                await session.SaveOrUpdateAsync(inst, cancellationToken).ConfigureAwait(false);
            }

            await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        var n            = command.NumberOfInstallments > 0 ? command.NumberOfInstallments : 4;
        var firstDueDate = command.FirstDueDate == default ? DateTime.Today : command.FirstDueDate;

        // Verifica integrità tabella millesimale abilitata
        var (millesimalTable, unitMillesimals) = await MillesimalTableGuard
            .LoadAndValidateAsync(session, budget.Condominium.Id, cancellationToken)
            .ConfigureAwait(false);

        // Ricalcola TotalIncome live dalle voci budget
        var totalIncome = await session.Query<BudgetItem>()
            .Where(x => x.Budget.Id == budget.Id && !x.IsDeleted)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        var totalMillesimal = unitMillesimals.Count > 0
            ? unitMillesimals.Sum(x => x.Millesimal)
            : (millesimalTable?.TotalMillesimal ?? 0m);
        var user = currentUser as CPQ.Core.Memberships.IUser;

        var openStatus = await session.Query<CondominiumInstallmentStatus>()
            .FirstOrDefaultAsync(x => x.Id == CondominiumInstallmentStatus.Open, cancellationToken)
            .ConfigureAwait(false);

        // Pre-calcola gli importi delle rate distribuendo il residuo sull'ultima
        var installmentAmounts = new decimal[n];
        var baseAmount = n > 0 ? Math.Round(totalIncome / n, 2) : 0m;
        var allocated = 0m;
        for (int i = 0; i < n - 1; i++) { installmentAmounts[i] = baseAmount; allocated += baseAmount; }
        installmentAmounts[n - 1] = totalIncome - allocated;

        for (int i = 1; i <= n; i++)
        {
            var dueDate             = firstDueDate.AddMonths(i - 1);
            var instAmount          = installmentAmounts[i - 1];
            var isLastInstallment   = i == n;

            var installment = new CondominiumInstallment
            {
                Condominium       = budget.Condominium,
                Budget            = budget,
                FiscalYear        = budget.FiscalYear,
                Tenant            = budget.Tenant,
                InstallmentNumber = i,
                DueDate           = dueDate,
                TotalAmount       = instAmount,
                Status            = openStatus,
                Notes             = $"Rata {i}/{n} – {budget.FiscalYear?.Code ?? dueDate.Year.ToString()}",
            };

            if (user != null) installment.Trace(user);

            await session.SaveOrUpdateAsync(installment, cancellationToken).ConfigureAwait(false);
            await session.FlushAsync(cancellationToken).ConfigureAwait(false);

            // Distribuisce le quote tra le unità, residuo sull'ultima unità
            var feeAllocated = 0m;
            for (int j = 0; j < unitMillesimals.Count; j++)
            {
                var um            = unitMillesimals[j];
                var isLastUnit    = j == unitMillesimals.Count - 1;
                decimal share;
                if (isLastUnit)
                    share = instAmount - feeAllocated;
                else
                {
                    share = totalMillesimal > 0
                        ? Math.Round(instAmount * um.Millesimal / totalMillesimal, 2)
                        : 0m;
                    feeAllocated += share;
                }

                var owner = await session.Query<UnitOwner>()
                    .FirstOrDefaultAsync(x => x.Unit.Id == um.Unit.Id && !x.IsDeleted, cancellationToken)
                    .ConfigureAwait(false);

                var fee = new CondominiumFee
                {
                    Installment   = installment,
                    Unit          = um.Unit,
                    UserId        = owner?.UserId ?? 0,
                    Tenant        = budget.Tenant,
                    AmountDue     = share,
                    AmountPaid    = 0m,
                    Balance       = share,
                    PaymentStatus = "ToPay",
                };

                if (user != null) fee.Trace(user);

                await session.SaveOrUpdateAsync(fee, cancellationToken).ConfigureAwait(false);
            }

            await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        return true;
    }
}
