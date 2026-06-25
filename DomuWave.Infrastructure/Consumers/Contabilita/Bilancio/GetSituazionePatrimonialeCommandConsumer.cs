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
/// Situazione patrimoniale alla data di fine esercizio (criterio di CASSA).
/// ATTIVITÀ : crediti v/condòmini (rate dovute non versate) + disponibilità liquide (conti liquidi).
/// PASSIVITÀ: debiti v/condòmini (saldi a credito) + debiti v/terzi (spese non pagate) + fondi.
/// Il bilancio dovrebbe chiudere in pareggio; eventuale scostamento è esposto come Sbilancio.
/// </summary>
public class GetSituazionePatrimonialeCommandConsumer
    : InMemoryConsumerBase<GetSituazionePatrimonialeCommand, SituazionePatrimonialeDto>
{
    private readonly IFiscalYearService _fiscalYearService;
    private readonly IUserService       _userService;

    public GetSituazionePatrimonialeCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IFiscalYearService fiscalYearService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _fiscalYearService = fiscalYearService;
        _userService       = userService;
    }

    protected override async Task<SituazionePatrimonialeDto> Consume(
        GetSituazionePatrimonialeCommand command,
        IMediationContext                mediationContext,
        CancellationToken                cancellationToken)
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

        // ── CREDITI / DEBITI v/CONDÒMINI ─────────────────────────────────────────
        // Balance = AmountDue - AmountPaid per ogni rata.
        //   Balance > 0 -> il condòmino deve versare  -> CREDITO del condominio (Attività)
        //   Balance < 0 -> il condòmino ha versato di più -> DEBITO del condominio (Passività)
        var balances = await session.Query<CondominiumFee>()
            .Where(f => f.Installment.Condominium.Id == condominiumId
                     && f.Installment.FiscalYear.Id  == fiscalYear.Id
                     && !f.IsDeleted)
            .Select(f => f.Balance)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var creditiVersoCondomini = balances.Where(b => b > 0).Sum();
        var debitiVersoCondomini  = balances.Where(b => b < 0).Sum() * -1m; // valore positivo

        // ── DISPONIBILITÀ LIQUIDE e FONDI (conti Patrimoniali) ───────────────────
        // Saldo del conto patrimoniale = OpeningBalance + movimenti (ClosingBalance se chiuso).
        var patrimonialBalances = await session.Query<AccountBalance>()
            .Where(b => b.FiscalYear.Id == fiscalYear.Id
                     && b.Account.Type == ChartOfAccountsType.Patrimoniale
                     && !b.IsDeleted)
            .Select(b => new
            {
                b.Account.Id,
                b.Account.Code,
                b.Account.Name,
                b.Account.IsLiquidity,
                Saldo = b.ClosingBalance != 0 ? b.ClosingBalance : b.OpeningBalance,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var disponibilita = patrimonialBalances
            .Where(b => b.IsLiquidity)
            .OrderBy(b => b.Code)
            .Select(b => new SituazionePatrimonialeRowDto
            {
                AccountId   = b.Id,
                AccountCode = b.Code,
                AccountName = b.Name,
                Amount      = b.Saldo,
            })
            .ToList();

        var fondi = patrimonialBalances
            .Where(b => !b.IsLiquidity)
            .OrderBy(b => b.Code)
            .Select(b => new SituazionePatrimonialeRowDto
            {
                AccountId   = b.Id,
                AccountCode = b.Code,
                AccountName = b.Name,
                Amount      = b.Saldo,
            })
            .ToList();

        // ── DEBITI v/TERZI: spese contabilizzate e non ancora pagate ─────────────
        var debitiVersoTerzi = await session.Query<Expense>()
            .Where(e => e.Condominium.Id == condominiumId
                     && e.FiscalYear.Id  == fiscalYear.Id
                     && e.PaymentStatus.Id != ExpensePaymentStatus.Pagata
                     && !e.IsDeleted)
            .SumAsync(e => (decimal?)e.GrossAmount, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        var totaleAttivita  = creditiVersoCondomini + disponibilita.Sum(d => d.Amount);
        var totalePassivita = debitiVersoCondomini + debitiVersoTerzi + fondi.Sum(f => f.Amount);

        return new SituazionePatrimonialeDto
        {
            FiscalYearId    = fiscalYear.Id,
            FiscalYearCode  = fiscalYear.Code,
            CondominiumId   = condominiumId,
            CondominiumName = fiscalYear.Condominium.Name,
            ReferenceDate   = fiscalYear.EndDate,

            CreditiVersoCondomini = creditiVersoCondomini,
            Disponibilita   = disponibilita,
            TotaleAttivita  = totaleAttivita,

            DebitiVersoCondomini = debitiVersoCondomini,
            DebitiVersoTerzi     = debitiVersoTerzi,
            Fondi           = fondi,
            TotalePassivita = totalePassivita,

            Sbilancio       = totaleAttivita - totalePassivita,
        };
    }
}
