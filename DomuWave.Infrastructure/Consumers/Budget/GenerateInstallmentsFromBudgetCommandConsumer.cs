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

        // Verifica che non esistano già rate per questo budget
        var existingCount = await session.Query<CondominiumInstallment>()
            .CountAsync(x => x.Budget.Id == budget.Id && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (existingCount > 0)
            throw new ValidatorException("Le rate per questo budget sono già state generate.");

        var n            = command.NumberOfInstallments > 0 ? command.NumberOfInstallments : 4;
        var firstDueDate = command.FirstDueDate == default ? DateTime.Today : command.FirstDueDate;

        // Carica la tabella millesimale attiva del condominio
        var millesimalTable = await session.Query<MillesimalTable>()
            .FirstOrDefaultAsync(x => x.Condominium.Id == budget.Condominium.Id
                                   && x.IsActive && !x.IsDraft && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        var unitMillesimals = millesimalTable != null
            ? await session.Query<UnitMillesimal>()
                .Where(x => x.MillesimalTable.Id == millesimalTable.Id && !x.IsDeleted)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false)
            : new List<UnitMillesimal>();

        var totalMillesimal      = millesimalTable?.TotalMillesimal ?? 0m;
        var amountPerInstallment = n > 0 ? Math.Round(budget.TotalIncome / n, 2) : 0m;
        var user                 = currentUser as CPQ.Core.Memberships.IUser;

        var openStatus = await session.Query<CondominiumInstallmentStatus>()
            .FirstOrDefaultAsync(x => x.Id == CondominiumInstallmentStatus.Open, cancellationToken)
            .ConfigureAwait(false);

        for (int i = 1; i <= n; i++)
        {
            var dueDate = firstDueDate.AddMonths(i - 1);

            var installment = new CondominiumInstallment
            {
                Condominium       = budget.Condominium,
                Budget            = budget,
                FiscalYear        = budget.FiscalYear,
                Tenant            = budget.Tenant,
                InstallmentNumber = i,
                DueDate           = dueDate,
                TotalAmount       = amountPerInstallment,
                Status            = openStatus,
                Notes             = $"Rata {i}/{n} – {budget.FiscalYear?.Code ?? dueDate.Year.ToString()}",
            };

            if (user != null) installment.Trace(user);

            await session.SaveOrUpdateAsync(installment, cancellationToken).ConfigureAwait(false);
            await session.FlushAsync(cancellationToken).ConfigureAwait(false);

            foreach (var um in unitMillesimals)
            {
                var share = totalMillesimal > 0
                    ? Math.Round(amountPerInstallment * um.Millesimal / totalMillesimal, 2)
                    : 0m;

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
