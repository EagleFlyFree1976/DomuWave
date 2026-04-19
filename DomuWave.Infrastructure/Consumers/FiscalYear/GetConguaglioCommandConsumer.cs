using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.FiscalYear;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetConguaglioCommandConsumer
    : InMemoryConsumerBase<GetConguaglioCommand, ConguaglioReadDto>
{
    private readonly IFiscalYearService _fiscalYearService;
    private readonly IUserService       _userService;

    public GetConguaglioCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IFiscalYearService fiscalYearService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _fiscalYearService = fiscalYearService;
        _userService       = userService;
    }

    protected override async Task<ConguaglioReadDto> Consume(
        GetConguaglioCommand command,
        IMediationContext    mediationContext,
        CancellationToken    cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var fiscalYear = await _fiscalYearService
            .GetByIdAsync(command.FiscalYearId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        if (fiscalYear == null)
            throw new NotFoundException("Esercizio fiscale non trovato.");

        // Recupera il budget consuntivo approvato (o chiuso) per l'esercizio
        var consuntivo = await session.Query<Budget>()
            .FirstOrDefaultAsync(b => b.FiscalYear.Id  == fiscalYear.Id
                                   && b.Type           == BudgetType.Consuntivo
                                   && (b.Status.Id == BudgetStatus.Approved || b.Status.Id == BudgetStatus.Closed)
                                   && !b.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (consuntivo == null)
            throw new ValidatorException(
                "Il conguaglio è disponibile solo dopo l'approvazione del budget Consuntivo.");

        // Totale spese reali = somma delle voci del consuntivo approvato
        var totalExpenses = await session.Query<BudgetItem>()
            .Where(i => i.Budget.Id == consuntivo.Id && !i.IsDeleted)
            .SumAsync(i => (decimal?)i.Amount, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        // Verifica integrità tabella millesimale abilitata
        var (millesimalTable, unitMillesimals) = await MillesimalTableGuard
            .LoadAndValidateAsync(session, fiscalYear.Condominium.Id, cancellationToken)
            .ConfigureAwait(false);

        var totalMillesimal = unitMillesimals.Sum(x => x.Millesimal);

        // Recupera il totale già pagato per unità tramite le CondominiumFee del preventivo
        var paidByUnit = await session.Query<CondominiumFee>()
            .Where(f => f.Installment.Budget.Condominium.Id == fiscalYear.Condominium.Id
                     && f.Installment.Budget.FiscalYear.Id  == fiscalYear.Id
                     && f.Installment.Budget.Type           == BudgetType.Preventivo
                     && !f.IsDeleted)
            .GroupBy(f => f.Unit.Id)
            .Select(g => new { UnitId = g.Key, TotalPaid = g.Sum(f => f.AmountPaid) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var paidMap = paidByUnit.ToDictionary(x => x.UnitId, x => x.TotalPaid);

        var totalPaid = paidMap.Values.Sum();

        // Carica i billing group del condominio con le unità associate
        var billingGroups = await session.Query<BillingGroup>()
            .Where(bg => bg.Condominium.Id == fiscalYear.Condominium.Id && !bg.IsDeleted)
            .FetchMany(bg => bg.Units)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Mappa unitId → groupId
        var unitToGroup = billingGroups
            .SelectMany(bg => bg.Units.Select(u => (UnitId: u.Id, Group: bg)))
            .ToDictionary(x => x.UnitId, x => x.Group);

        // Costruisce le righe individuali per ogni unità millesimale
        ConguaglioUnitItemDto BuildUnitRow(UnitMillesimal um)
        {
            var quotaConsuntiva = totalMillesimal > 0
                ? Math.Round(totalExpenses * um.Millesimal / totalMillesimal, 2)
                : 0m;
            var alreadyPaid = paidMap.TryGetValue(um.Unit.Id, out var p) ? p : 0m;
            var saldo       = quotaConsuntiva - alreadyPaid;
            return new ConguaglioUnitItemDto
            {
                UnitId             = um.Unit.Id,
                UnitInternalNumber = um.Unit.InternalNumber,
                UnitDescription    = um.Unit.FormatUnitName(),
                Millesimal         = um.Millesimal,
                QuotaConsuntiva    = quotaConsuntiva,
                AlreadyPaid        = alreadyPaid,
                Saldo              = saldo,
                SaldoType          = saldo > 0 ? "Debit" : saldo < 0 ? "Credit" : "Even",
            };
        }

        // Raggruppa: unità con gruppo → aggregato; unità senza gruppo → individuale
        var unitItems = new List<ConguaglioUnitItemDto>();
        var grouped   = new Dictionary<int, List<ConguaglioUnitItemDto>>(); // groupId → sub-rows

        foreach (var um in unitMillesimals.OrderBy(x => x.Unit?.InternalNumber))
        {
            var row = BuildUnitRow(um);
            if (unitToGroup.TryGetValue(um.Unit.Id, out var grp))
            {
                if (!grouped.ContainsKey(grp.Id)) grouped[grp.Id] = new List<ConguaglioUnitItemDto>();
                grouped[grp.Id].Add(row);
            }
            else
            {
                unitItems.Add(row);
            }
        }

        // Costruisce le righe aggregate per ogni gruppo
        foreach (var (groupId, subRows) in grouped.OrderBy(kv => billingGroups.First(bg => bg.Id == kv.Key).Name))
        {
            var grp  = billingGroups.First(bg => bg.Id == groupId);
            var saldo = subRows.Sum(r => r.Saldo);
            unitItems.Add(new ConguaglioUnitItemDto
            {
                UnitId             = 0,
                UnitInternalNumber = string.Empty,
                UnitDescription    = grp.Name,
                IsGroup            = true,
                BillingGroupId     = grp.Id,
                BillingGroupName   = grp.Name,
                Millesimal         = subRows.Sum(r => r.Millesimal),
                QuotaConsuntiva    = subRows.Sum(r => r.QuotaConsuntiva),
                AlreadyPaid        = subRows.Sum(r => r.AlreadyPaid),
                Saldo              = saldo,
                SaldoType          = saldo > 0 ? "Debit" : saldo < 0 ? "Credit" : "Even",
                Units              = subRows,
            });
        }

        unitItems = unitItems.OrderBy(r => r.IsGroup ? r.BillingGroupName : r.UnitInternalNumber).ToList();

        return new ConguaglioReadDto
        {
            FiscalYearId    = fiscalYear.Id,
            FiscalYearCode  = fiscalYear.Code,
            CondominiumId   = fiscalYear.Condominium.Id,
            CondominiumName = fiscalYear.Condominium.Name,
            TotalExpenses   = totalExpenses,
            TotalPaid       = totalPaid,
            GlobalBalance   = totalExpenses - totalPaid,
            ApprovalDate    = consuntivo.ApprovalDate,
            Units           = unitItems,
        };
    }
}
