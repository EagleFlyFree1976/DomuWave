using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Contabilita.Bilancio;
using DomuWave.Services.Dto.Contabilita.Bilancio;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

/// <summary>
/// Flussi di cassa per CASSA: incassi e pagamenti effettivamente avvenuti nel periodo.
/// Avanzo iniziale = OpeningBalance dei conti liquidi dell'esercizio.
/// Incassi  = CondominiumFee.AmountPaid con PaymentDate nel periodo.
/// Pagamenti = Expense.GrossAmount con PaymentDate nel periodo, distinte per esercizio
///             di competenza (corrente / precedenti) e per imputazione (condominiale / individuale).
/// </summary>
public class GetFlussiCassaCommandConsumer
    : InMemoryConsumerBase<GetFlussiCassaCommand, FlussiCassaDto>
{
    private readonly IFiscalYearService _fiscalYearService;
    private readonly IUserService       _userService;

    public GetFlussiCassaCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IFiscalYearService fiscalYearService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _fiscalYearService = fiscalYearService;
        _userService       = userService;
    }

    protected override async Task<FlussiCassaDto> Consume(
        GetFlussiCassaCommand command,
        IMediationContext     mediationContext,
        CancellationToken     cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var fiscalYear = await _fiscalYearService
            .GetByIdAsync(command.FiscalYearId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        if (fiscalYear == null)
            throw new NotFoundException("Esercizio fiscale non trovato.");

        var condominiumId = fiscalYear.Condominium.Id;
        var start = fiscalYear.StartDate;
        var end   = fiscalYear.EndDate;

        // ── AVANZO INIZIALE DI CASSA: somma OpeningBalance dei conti liquidi ──────
        var avanzoIniziale = await session.Query<AccountBalance>()
            .Where(b => b.FiscalYear.Id == fiscalYear.Id
                     && b.Account.Type == ChartOfAccountsType.Patrimoniale
                     && b.Account.IsLiquidity
                     && !b.IsDeleted)
            .SumAsync(b => (decimal?)b.OpeningBalance, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        // ── INCASSI: versamenti dei condòmini con data di pagamento nel periodo ───
        var versamenti = await session.Query<CondominiumFee>()
            .Where(f => f.Installment.Condominium.Id == condominiumId
                     && f.PaymentDate != null
                     && f.PaymentDate >= start
                     && f.PaymentDate <= end
                     && !f.IsDeleted)
            .SumAsync(f => (decimal?)f.AmountPaid, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        // ── PAGAMENTI: spese con data di pagamento nel periodo ────────────────────
        var paidExpenses = await session.Query<Expense>()
            .Where(e => e.Condominium.Id == condominiumId
                     && e.PaymentDate != null
                     && e.PaymentDate >= start
                     && e.PaymentDate <= end
                     && !e.IsDeleted)
            .Select(e => new
            {
                e.GrossAmount,
                FiscalYearId = (int?)e.FiscalYear.Id,
                IsIndividual = e.Unit != null,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Uscite individuali: spese imputate a singola unità.
        var usciteIndividuali = paidExpenses.Where(e => e.IsIndividual).Sum(e => e.GrossAmount);

        // Spese condominiali, distinte per esercizio di competenza.
        var condominiali = paidExpenses.Where(e => !e.IsIndividual).ToList();
        var pagamentiCorrente  = condominiali
            .Where(e => e.FiscalYearId == fiscalYear.Id).Sum(e => e.GrossAmount);
        var pagamentiPrecedenti = condominiali
            .Where(e => e.FiscalYearId != fiscalYear.Id).Sum(e => e.GrossAmount);

        var totaleIncassi   = avanzoIniziale + versamenti;
        var totalePagamenti = pagamentiCorrente + pagamentiPrecedenti + usciteIndividuali;

        return new FlussiCassaDto
        {
            FiscalYearId    = fiscalYear.Id,
            FiscalYearCode  = fiscalYear.Code,
            CondominiumId   = condominiumId,
            CondominiumName = fiscalYear.Condominium.Name,
            StartDate       = start,
            EndDate         = end,

            AvanzoInizialeCassa = avanzoIniziale,
            VersamentiCondomini = versamenti,
            TotaleIncassi       = totaleIncassi,

            PagamentiEsercizioCorrente  = pagamentiCorrente,
            PagamentiEserciziPrecedenti = pagamentiPrecedenti,
            UsciteIndividuali           = usciteIndividuali,
            TotalePagamenti = totalePagamenti,

            AvanzoFinaleCassa = totaleIncassi - totalePagamenti,
        };
    }
}
