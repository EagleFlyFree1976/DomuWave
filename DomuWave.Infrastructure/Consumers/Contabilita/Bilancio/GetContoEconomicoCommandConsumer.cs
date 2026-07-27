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
/// Conto economico per COMPETENZA.
/// Entrate  = versamenti dei condòmini con data di pagamento nel periodo dell'esercizio.
/// Uscite   = spese contabilizzate nell'esercizio (per FiscalYear), pagate o meno,
///            raggruppate per conto di tipo Uscita.
/// Saldo es. prec. = somma OpeningBalance dei conti dell'esercizio precedente (se presente).
/// </summary>
public class GetContoEconomicoCommandConsumer
    : InMemoryConsumerBase<GetContoEconomicoCommand, ContoEconomicoDto>
{
    private readonly IFiscalYearService _fiscalYearService;
    private readonly IUserService       _userService;

    public GetContoEconomicoCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IFiscalYearService fiscalYearService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _fiscalYearService = fiscalYearService;
        _userService       = userService;
    }

    protected override async Task<ContoEconomicoDto> Consume(
        GetContoEconomicoCommand command,
        IMediationContext        mediationContext,
        CancellationToken        cancellationToken)
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

        // ── ENTRATE: versamenti dei condòmini (per cassa nel periodo di competenza) ──
        // Le rate manuali hanno Installment.Budget == null: vanno conteggiate.
        var versamenti = await session.Query<CondominiumFee>()
            .Where(f => f.Installment.Condominium.Id == condominiumId
                     && f.Installment.FiscalYear.Id  == fiscalYear.Id
                     && !f.IsDeleted
                     && !f.Installment.IsDeleted)
            .SumAsync(f => (decimal?)f.AmountPaid, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        // ── ALTRE ENTRATE: movimenti su conti di tipo Entrata (rimborsi, interessi, ecc.) ──
        // Sono registrati come Expense ma imputati a un conto Entrata: vanno a Entrate, non a Uscite.
        var entrateRows = (await session.Query<Expense>()
            .Where(e => e.Condominium.Id == condominiumId
                     && e.FiscalYear.Id  == fiscalYear.Id
                     && e.Account.Type   == ChartOfAccountsType.Entrata
                     && !e.IsDeleted)
            .GroupBy(e => new { e.Account.Id, e.Account.Code, e.Account.Name })
            .Select(g => new ContoEconomicoRowDto
            {
                AccountId   = g.Key.Id,
                AccountCode = g.Key.Code,
                AccountName = g.Key.Name,
                Amount      = g.Sum(e => e.GrossAmount),
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false))
            .OrderBy(r => r.AccountCode)
            .ToList();

        var altreEntrate = entrateRows.Sum(r => r.Amount);

        // ── USCITE: spese per competenza dell'esercizio, raggruppate per conto Uscita ──
        // Importo = GrossAmount + WithholdingTax (base lorda): la ritenuta d'acconto è a
        // carico dei condòmini, quindi il costo di competenza la include. Così il totale
        // riconcilia con la base di ripartizione del Bilancio di ripartizione.
        var expenseRows = await session.Query<Expense>()
            .Where(e => e.Condominium.Id == condominiumId
                     && e.FiscalYear.Id  == fiscalYear.Id
                     && e.Account.Type   == ChartOfAccountsType.Uscita
                     && !e.IsDeleted)
            .GroupBy(e => new { e.Account.Id, e.Account.Code, e.Account.Name })
            .Select(g => new ContoEconomicoRowDto
            {
                AccountId   = g.Key.Id,
                AccountCode = g.Key.Code,
                AccountName = g.Key.Name,
                Amount      = g.Sum(e => e.GrossAmount + e.WithholdingTax),
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var usciteRows  = expenseRows.OrderBy(r => r.AccountCode).ToList();
        var totaleUscite = usciteRows.Sum(r => r.Amount);

        // ── SALDO ESERCIZIO PRECEDENTE ──────────────────────────────────────────
        // Convenzione: positivo = credito a favore del condominio; negativo = debito.
        decimal saldoPrec = 0m;
        if (fiscalYear.PreviousFiscalYear != null)
        {
            saldoPrec = await session.Query<AccountBalance>()
                .Where(b => b.FiscalYear.Id == fiscalYear.PreviousFiscalYear.Id && !b.IsDeleted)
                .SumAsync(b => (decimal?)b.ClosingBalance, cancellationToken)
                .ConfigureAwait(false) ?? 0m;
        }

        var totaleEntrate = versamenti + altreEntrate;
        var saldoFinale   = totaleEntrate - totaleUscite + saldoPrec;

        return new ContoEconomicoDto
        {
            FiscalYearId    = fiscalYear.Id,
            FiscalYearCode  = fiscalYear.Code,
            CondominiumId   = condominiumId,
            CondominiumName = fiscalYear.Condominium.Name,
            StartDate       = fiscalYear.StartDate,
            EndDate         = fiscalYear.EndDate,

            VersamentiCondomini = versamenti,
            EntrateRows         = entrateRows,
            TotaleEntrate       = totaleEntrate,

            UsciteRows      = usciteRows,
            TotaleUscite    = totaleUscite,

            SaldoEsercizioPrecedente = saldoPrec,
            SaldoFinale     = saldoFinale,
            SaldoTipo       = saldoFinale > 0 ? "Avanzo" : saldoFinale < 0 ? "Disavanzo" : "Pareggio",
        };
    }
}
