using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.FiscalYear;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetExpensesByMillesimalReportCommandConsumer
    : InMemoryConsumerBase<GetExpensesByMillesimalReportCommand, ExpensesByMillesimalReportDto>
{
    private readonly IUserService _userService;

    public GetExpensesByMillesimalReportCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<ExpensesByMillesimalReportDto> Consume(
        GetExpensesByMillesimalReportCommand command,
        IMediationContext                    mediationContext,
        CancellationToken                    cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var fiscalYear = await session.Query<Models.FiscalYear>()
            .FirstOrDefaultAsync(f => f.Id == command.FiscalYearId && !f.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (fiscalYear == null)
            throw new NotFoundException("Esercizio fiscale non trovato.");

        // Conti associati a un tipo di consumo: le relative spese sono ripartite per
        // consumi (non a millesimi) e vanno in una sezione dedicata del report.
        var condominiumId = fiscalYear.Condominium?.Id ?? 0;
        var consumoAccountIds = (await session.Query<ConsumptionType>()
                .Where(t => t.Condominium.Id == condominiumId && !t.IsDeleted && t.Account != null)
                .Select(t => t.Account.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false))
            .ToHashSet();

        var rows = (await session.Query<Expense>()
            .Where(e => e.FiscalYear.Id == command.FiscalYearId && !e.IsDeleted)
            .Select(e => new
            {
                e.Id,
                e.DocumentDate,
                e.DocumentNumber,
                e.Name,
                SupplierName        = e.Supplier != null ? e.Supplier.Name : null,
                AccountId           = e.Account  != null ? (int?)e.Account.Id : null,
                AccountName         = e.Account  != null ? e.Account.Name  : null,
                MillesimalTableId   = e.MillesimalTable != null ? (int?)e.MillesimalTable.Id : null,
                MillesimalTableName = e.MillesimalTable != null ? e.MillesimalTable.Name : null,
                MillesimalTableCode = e.MillesimalTable != null ? e.MillesimalTable.Code : null,
                UnitId              = e.Unit != null ? (int?)e.Unit.Id : null,
                UnitName            = e.Unit != null ? (e.Unit.DisplayName ?? e.Unit.Name ?? e.Unit.InternalNumber) : null,
                GrossAmount = e.GrossAmount + e.WithholdingTax,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false))
            .Select(e => new
            {
                e.Id, e.DocumentDate, e.DocumentNumber, e.Name, e.SupplierName,
                e.AccountId, e.AccountName, e.MillesimalTableId, e.MillesimalTableName,
                e.MillesimalTableCode, e.UnitId, e.UnitName, e.GrossAmount,
                IsConsumption = e.AccountId.HasValue && consumoAccountIds.Contains(e.AccountId.Value),
            })
            .ToList();

        var groups = new List<ExpensesByMillesimalGroupDto>();

        // ── Gruppi per tabella millesimale (escluse le spese a consumo) ─────
        var byTable = rows
            .Where(r => !r.IsConsumption && r.MillesimalTableId.HasValue)
            .GroupBy(r => r.MillesimalTableId!.Value)
            .OrderBy(g => g.First().MillesimalTableCode ?? g.First().MillesimalTableName);

        foreach (var g in byTable)
        {
            var first = g.First();
            var label = !string.IsNullOrWhiteSpace(first.MillesimalTableName)
                ? first.MillesimalTableName!
                : (first.MillesimalTableCode ?? "Tabella millesimale");

            groups.Add(new ExpensesByMillesimalGroupDto
            {
                MillesimalTableId = g.Key,
                GroupName         = label,
                IsDirectUnit      = false,
                Expenses          = g
                    .OrderBy(r => r.DocumentDate ?? DateTime.MaxValue)
                    .Select(r => new ExpenseReportRowDto
                    {
                        ExpenseId      = r.Id,
                        DocumentDate   = r.DocumentDate,
                        DocumentNumber = r.DocumentNumber,
                        Name           = r.Name ?? string.Empty,
                        SupplierName   = r.SupplierName,
                        AccountName    = r.AccountName,
                        GrossAmount    = r.GrossAmount,
                    }).ToList(),
                GroupTotal        = g.Sum(r => r.GrossAmount),
            });
        }

        // ── Gruppo spese imputate direttamente a un immobile (escluse a consumo) ─
        var directRows = rows.Where(r => !r.IsConsumption && !r.MillesimalTableId.HasValue && r.UnitId.HasValue).ToList();
        if (directRows.Any())
        {
            groups.Add(new ExpensesByMillesimalGroupDto
            {
                MillesimalTableId = null,
                GroupName         = "Imputazione diretta a immobile",
                IsDirectUnit      = true,
                Expenses          = directRows
                    .OrderBy(r => r.UnitName)
                    .ThenBy(r => r.DocumentDate ?? DateTime.MaxValue)
                    .Select(r => new ExpenseReportRowDto
                    {
                        ExpenseId      = r.Id,
                        DocumentDate   = r.DocumentDate,
                        DocumentNumber = r.DocumentNumber,
                        Name           = r.Name ?? string.Empty,
                        SupplierName   = r.SupplierName,
                        AccountName    = r.AccountName,
                        UnitName       = r.UnitName,
                        GrossAmount    = r.GrossAmount,
                    }).ToList(),
                GroupTotal        = directRows.Sum(r => r.GrossAmount),
            });
        }

        // ── Gruppo spese ripartite per consumi (conti a consumo) ────────────
        var consumptionRows = rows.Where(r => r.IsConsumption).ToList();
        if (consumptionRows.Any())
        {
            groups.Add(new ExpensesByMillesimalGroupDto
            {
                MillesimalTableId = null,
                GroupName         = "Spese ripartite per consumi",
                IsConsumption     = true,
                Expenses          = consumptionRows
                    .OrderBy(r => r.AccountName)
                    .ThenBy(r => r.DocumentDate ?? DateTime.MaxValue)
                    .Select(r => new ExpenseReportRowDto
                    {
                        ExpenseId      = r.Id,
                        DocumentDate   = r.DocumentDate,
                        DocumentNumber = r.DocumentNumber,
                        Name           = r.Name ?? string.Empty,
                        SupplierName   = r.SupplierName,
                        AccountName    = r.AccountName,
                        GrossAmount    = r.GrossAmount,
                    }).ToList(),
                GroupTotal        = consumptionRows.Sum(r => r.GrossAmount),
            });
        }

        return new ExpensesByMillesimalReportDto
        {
            FiscalYearId    = fiscalYear.Id,
            FiscalYearCode  = fiscalYear.Code,
            CondominiumId   = fiscalYear.Condominium?.Id ?? 0,
            CondominiumName = fiscalYear.Condominium?.Name,
            Groups          = groups,
            GrandTotal      = rows.Sum(r => r.GrossAmount),
        };
    }
}
