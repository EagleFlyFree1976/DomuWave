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

        // Saldo iniziale di cassa del condominio: confluisce SOLO per il primo esercizio
        // (per quelli successivi la liquidità di apertura deriva dalla chiusura precedente).
        var isFirstFiscalYear = !await session.Query<FiscalYear>()
            .AnyAsync(f => f.Condominium.Id == condominiumId
                        && f.Id != fiscalYear.Id
                        && f.StartDate < fiscalYear.StartDate
                        && !f.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (isFirstFiscalYear)
            avanzoIniziale += fiscalYear.Condominium.InitialBalance;

        // ── INCASSI: versamenti dei condòmini con data di pagamento nel periodo ───
        var versamenti = await session.Query<CondominiumFee>()
            .Where(f => f.Installment.Condominium.Id == condominiumId
                     && f.PaymentDate != null
                     && f.PaymentDate >= start
                     && f.PaymentDate <= end
                     && !f.IsDeleted
                     && !f.Installment.IsDeleted)
            .SumAsync(f => (decimal?)f.AmountPaid, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        // ── MOVIMENTI con data di pagamento nel periodo (incassi + pagamenti) ─────
        var paidMovements = await session.Query<Expense>()
            .Where(e => e.Condominium.Id == condominiumId
                     && e.PaymentDate != null
                     && e.PaymentDate >= start
                     && e.PaymentDate <= end
                     && !e.IsDeleted)
            .Select(e => new
            {
                e.GrossAmount,
                AccountType  = e.Account.Type,
                FiscalYearId = (int?)e.FiscalYear.Id,
                IsIndividual = e.Unit != null,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Movimenti su conti di tipo Entrata: sono incassi, non pagamenti.
        var altriIncassi = paidMovements
            .Where(e => e.AccountType == ChartOfAccountsType.Entrata)
            .Sum(e => e.GrossAmount);

        // ── PAGAMENTI: solo movimenti su conti di tipo Uscita ─────────────────────
        var paidExpenses = paidMovements
            .Where(e => e.AccountType == ChartOfAccountsType.Uscita)
            .ToList();

        // Uscite individuali: spese imputate a singola unità.
        var usciteIndividuali = paidExpenses.Where(e => e.IsIndividual).Sum(e => e.GrossAmount);

        // Spese condominiali, distinte per esercizio di competenza.
        var condominiali = paidExpenses.Where(e => !e.IsIndividual).ToList();
        var pagamentiCorrente  = condominiali
            .Where(e => e.FiscalYearId == fiscalYear.Id).Sum(e => e.GrossAmount);
        var pagamentiPrecedenti = condominiali
            .Where(e => e.FiscalYearId != fiscalYear.Id).Sum(e => e.GrossAmount);

        // Totale incassi = solo gli incassi effettivi del periodo (NON l'avanzo iniziale,
        // che è il saldo di partenza ed è esposto come riga a sé).
        var totaleIncassi   = versamenti + altriIncassi;
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
            AltriIncassi        = altriIncassi,
            TotaleIncassi       = totaleIncassi,

            PagamentiEsercizioCorrente  = pagamentiCorrente,
            PagamentiEserciziPrecedenti = pagamentiPrecedenti,
            UsciteIndividuali           = usciteIndividuali,
            TotalePagamenti = totalePagamenti,

            // Avanzo finale = avanzo iniziale + incassi del periodo − pagamenti del periodo.
            AvanzoFinaleCassa = avanzoIniziale + totaleIncassi - totalePagamenti,
        };
    }
}
