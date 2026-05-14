using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Admin;
using DomuWave.Services.Dto.Admin;
using NHibernate;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Admin;

public class AlignHiloCommandConsumer
    : InMemoryConsumerBase<AlignHiloCommand, IList<HiloAlignmentResultDto>>
{
    private readonly IUserService _userService;

    private static readonly (string EntityType, string TableName, string IdColumn, int MaxLo)[] Definitions =
    [
        ("AccountBalance",                 "AccountBalance",                 "Id",          10),
        ("Assembly",                        "Assembly",                        "Id",          10),
        ("AssemblyAgendaItem",              "AssemblyAgendaItem",              "Id",          10),
        ("AssemblyAttendance",              "AssemblyAttendance",              "Id",          10),
        ("BillingGroup",                    "BillingGroup",                    "Id",          10),
        ("Budget",                          "Budget",                          "Id",          10),
        ("BudgetItem",                      "BudgetItem",                      "Id",          10),
        ("Building",                        "Building",                        "Id",          10),
        ("ChartOfAccounts",                 "ChartOfAccounts",                 "Id",          10),
        ("ChartOfAccountsCategory",         "ChartOfAccountsCategory",         "Id",          10),
        ("ChartOfAccountsCategoryTemplate", "ChartOfAccountsCategoryTemplate", "Id",          10),
        ("ChartOfAccountsTemplate",         "ChartOfAccountsTemplate",         "Id",          10),
        ("ChartOfAccountsTemplateItem",     "ChartOfAccountsTemplateItem",     "Id",          10),
        ("Communication",                   "Communication",                   "Id",          10),
        ("CommunicationNotification",       "CommunicationNotification",       "Id",          10),
        ("CommunicationRead",               "CommunicationRead",               "Id",          10),
        ("Condominium",                     "Condominium",                     "Id",          10),
        ("CondominiumAddress",              "CondominiumAddress",              "Id",          10),
        ("CondominiumCadastralData",        "CondominiumCadastralData",        "Id",          10),
        ("CondominiumFee",                  "CondominiumFee",                  "Id",          10),
        ("CondominiumInstallment",          "CondominiumInstallment",          "Id",          10),
        ("ConsumptionCharge",               "ConsumptionCharge",               "Id",          10),
        ("ConsumptionChargeItem",           "ConsumptionChargeItem",           "Id",          10),
        ("ConsumptionReading",              "ConsumptionReading",              "Id",          10),
        ("ConsumptionType",                 "ConsumptionType",                 "Id",          10),
        ("Document",                        "Document",                        "Id",          10),
        ("DocumentAccess",                  "DocumentAccess",                  "Id",          10),
        ("DynamicFile",                     "DynamicFile",                     "Id",          10),
        ("DynamicFileContent",              "DynamicFileContent",              "Id",          10),
        ("Expense",                         "Expense",                         "Id",          10),
        ("ExpenseAllocation",               "ExpenseAllocation",               "Id",          10),
        ("ExtraordinaryWork",               "ExtraordinaryWork",               "Id",          10),
        ("FiscalYear",                      "FiscalYear",                      "FiscalYearId", 100),
        ("Maintenance",                     "Maintenance",                     "Id",          10),
        ("MenuItem",                        "base_menues",                     "MenuId",      32767),
        ("Message",                         "Message",                         "Id",          10),
        ("Meter",                           "Meter",                           "Id",          10),
        ("MillesimalTable",                 "MillesimalTable",                 "Id",          10),
        ("NotificationTemplate",            "NotificationTemplate",            "Id",          10),
        ("Payment",                         "Payment",                         "PaymentId",   10),
        ("RealEstateUnit",                  "RealEstateUnit",                  "Id",          10),
        ("Receipt",                         "Receipt",                         "Id",          10),
        ("Staircase",                       "Staircase",                       "Id",          10),
        ("Supplier",                        "Supplier",                        "Id",          10),
        ("SupplierContract",                "SupplierContract",                "Id",          10),
        ("TenantSmtpSettings",              "TenantSmtpSettings",              "Id",          10),
        ("UnitMillesimal",                  "UnitMillesimal",                  "Id",          10),
        ("UnitOpeningBalance",              "UnitOpeningBalance",              "Id",          10),
        ("UnitOwner",                       "UnitOwner",                       "Id",          10),
        ("UnitTenant",                      "UnitTenant",                      "Id",          10),
        ("UserTenant",                      "UserTenant",                      "Id",          10),
        ("WorkQuote",                       "WorkQuote",                       "Id",          10),
        ("WorkQuoteDocument",               "WorkQuoteDocument",               "Id",          10),
    ];

    public AlignHiloCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<IList<HiloAlignmentResultDto>> Consume(
        AlignHiloCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var results = new List<HiloAlignmentResultDto>();

        foreach (var (entityType, tableName, idColumn, maxLo) in Definitions)
        {
            var maxIdSql = $"SELECT ISNULL(MAX(CAST([{idColumn}] AS BIGINT)), 0) FROM dbo.[{tableName}]";
            var maxId = await session.CreateSQLQuery(maxIdSql)
                .UniqueResultAsync<long>(cancellationToken)
                .ConfigureAwait(false);

            long block = (long)maxLo + 1;
            int computed = maxId <= 0
                ? 1
                : (int)(Math.Floor((double)maxId / block) + 1);

            var hiloRow = await session.CreateSQLQuery(
                    "SELECT next_hi FROM dbo.hibernate_unique_key WHERE entity_type = :et")
                .SetParameter("et", entityType)
                .UniqueResultAsync<int?>(cancellationToken)
                .ConfigureAwait(false);

            int final;
            string action;

            if (hiloRow == null)
            {
                await session.CreateSQLQuery(
                        "INSERT INTO dbo.hibernate_unique_key (next_hi, entity_type) VALUES (:v, :et)")
                    .SetParameter("v", computed)
                    .SetParameter("et", entityType)
                    .ExecuteUpdateAsync(cancellationToken)
                    .ConfigureAwait(false);
                final  = computed;
                action = "INSERT";
            }
            else
            {
                final = hiloRow.Value < computed ? computed : hiloRow.Value;
                await session.CreateSQLQuery(
                        "UPDATE dbo.hibernate_unique_key SET next_hi = :v WHERE entity_type = :et")
                    .SetParameter("v", final)
                    .SetParameter("et", entityType)
                    .ExecuteUpdateAsync(cancellationToken)
                    .ConfigureAwait(false);
                action = "UPDATE";
            }

            results.Add(new HiloAlignmentResultDto
            {
                EntityType     = entityType,
                TableName      = tableName,
                IdColumn       = idColumn,
                MaxId          = maxId,
                MaxLo          = maxLo,
                ComputedNextHi = computed,
                OldNextHi      = hiloRow,
                FinalNextHi    = final,
                Action         = action,
            });
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        return results.OrderBy(r => r.EntityType).ToList();
    }
}
