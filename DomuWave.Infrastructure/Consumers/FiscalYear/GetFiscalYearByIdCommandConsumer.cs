using DomuWave.Services.Models;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using DomuWave.Services.Interfaces.Extensions;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.FiscalYear;
using DomuWave.Services.Interfaces;
using NHibernate.Linq;
using SimpleMediator.Core;
using System.Linq;
using Models = DomuWave.Services.Models;

namespace DomuWave.Services.Consumers;

public class GetFiscalYearByIdCommandConsumer : InMemoryConsumerBase<GetFiscalYearByIdCommand, FiscalYearReadDto>
{
    private readonly IFiscalYearService _fiscalYearService;
    private readonly IUserService       _userService;

    public GetFiscalYearByIdCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IFiscalYearService fiscalYearService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _fiscalYearService = fiscalYearService;
        _userService       = userService;
    }

    protected override async Task<FiscalYearReadDto> Consume(
        GetFiscalYearByIdCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var fy = await _fiscalYearService
            .GetByIdAsync(command.FiscalYearId, command.TenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        if (fy == null) return null;

        var dto = fy.ToReadDto();

        // ── Calcola riepilogo finanziario ──────────────────────────────
        var fyId = fy.Id;

        var installments = await session.Query<CondominiumInstallment>()
            .Where(i => i.FiscalYear.Id == fyId && !i.IsDeleted)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var installmentIds = installments.Select(i => i.Id).ToList();

        var fees = installmentIds.Any()
            ? await session.Query<CondominiumFee>()
                .Where(f => installmentIds.Contains(f.Installment.Id) && !f.IsDeleted && !f.Installment.IsDeleted)
                .ToListAsync(cancellationToken).ConfigureAwait(false)
            : new System.Collections.Generic.List<CondominiumFee>();

        var expenses = await session.Query<Expense>()
            .Where(e => e.FiscalYear.Id == fyId && !e.IsDeleted)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var budgetCount = await session.Query<Budget>()
            .CountAsync(b => b.FiscalYear.Id == fyId && !b.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        var totalOpeningBalance = await session.Query<Models.UnitOpeningBalance>()
            .Where(b => b.FiscalYear.Id == fyId && !b.IsDeleted)
            .SumAsync(b => (decimal?)b.OpeningBalance, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        // Costo a carico dei condòmini = totale fattura = GrossAmount (già al netto
        // della ritenuta) + ritenuta d'acconto. Coerente con report spese per millesimale,
        // bilancio di ripartizione e allocazioni.
        decimal Chargeable(Expense e) => e.GrossAmount + e.WithholdingTax;

        dto.Summary = new FiscalYearSummaryDto
        {
            TotalOpeningBalance      = totalOpeningBalance,
            TotalExpenses            = expenses.Sum(Chargeable),
            TotalExpensesPaid        = expenses.Where(e => e.PaymentStatus?.Id == ExpensePaymentStatus.Pagata).Sum(Chargeable),
            TotalInstallmentsBilled  = installments.Sum(i => i.TotalAmount),
            TotalPaymentsReceived    = fees.Sum(f => f.AmountPaid),
            Balance                  = totalOpeningBalance + fees.Sum(f => f.AmountPaid) - expenses.Sum(Chargeable),
            ExpenseCount             = expenses.Count,
            InstallmentCount         = installments.Count,
            BudgetCount              = budgetCount,
        };

        return dto;
    }
}
