using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.AI.Models;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;

namespace DomuWave.Services.AI
{
    /// <summary>
    /// Esegue il routing di un tool richiesto dal modello verso il service layer esistente.
    /// Restituisce solo dati aggregati/strutturati (vedi nota GDPR del piano).
    /// </summary>
    public class AiToolDispatcher
    {
        private readonly ICondominiumFeeService _feeService;
        private readonly ICondominiumInstallmentService _installmentService;
        private readonly IExpenseService _expenseService;
        private readonly IBudgetService _budgetService;
        private readonly ICondominiumService _condominiumService;

        public AiToolDispatcher(
            ICondominiumFeeService feeService,
            ICondominiumInstallmentService installmentService,
            IExpenseService expenseService,
            IBudgetService budgetService,
            ICondominiumService condominiumService)
        {
            _feeService = feeService;
            _installmentService = installmentService;
            _expenseService = expenseService;
            _budgetService = budgetService;
            _condominiumService = condominiumService;
        }

        public async Task<AiToolResult> DispatchAsync(
            string toolName,
            JsonElement input,
            Guid tenantId,
            int? defaultYear,
            IUser currentUser,
            CancellationToken ct)
        {
            try
            {
                return toolName switch
                {
                    "get_condomino_payments" => await GetCondominoPayments(input, tenantId, defaultYear, currentUser, ct),
                    "get_overdue_fees"       => await GetOverdueFees(input, currentUser, ct),
                    "get_installment_status" => await GetInstallmentStatus(input, currentUser, ct),
                    "get_expense_summary"    => await GetExpenseSummary(input, tenantId, defaultYear, currentUser, ct),
                    "get_budget_vs_actual"   => await GetBudgetVsActual(input, tenantId, defaultYear, currentUser, ct),
                    "get_condominium_list"   => await GetCondominiumList(tenantId, currentUser, ct),
                    "get_owner_balance"      => await GetOwnerBalance(input, tenantId, currentUser, ct),
                    _ => AiToolResult.Error(toolName, $"Tool sconosciuto: {toolName}")
                };
            }
            catch (Exception ex)
            {
                return AiToolResult.Error(toolName, $"Errore nell'esecuzione del tool: {ex.Message}");
            }
        }

        // ─── Helpers di parsing input ─────────────────────────────────────────
        private static string GetString(JsonElement input, string name)
            => input.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

        private static int? GetInt(JsonElement input, string name)
            => input.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : null;

        private static bool GetBool(JsonElement input, string name)
            => input.TryGetProperty(name, out var v) && (v.ValueKind == JsonValueKind.True);

        // ─── Tool implementations ─────────────────────────────────────────────
        private async Task<AiToolResult> GetCondominoPayments(
            JsonElement input, Guid tenantId, int? defaultYear, IUser currentUser, CancellationToken ct)
        {
            var ownerName = GetString(input, "owner_name");
            if (string.IsNullOrWhiteSpace(ownerName))
                return AiToolResult.Error("get_condomino_payments", "Parametro owner_name mancante.");

            var condominiumId = GetInt(input, "condominium_id");
            var year = GetInt(input, "year") ?? defaultYear ?? DateTime.Now.Year;

            var fees = await _feeService.GetFeesByOwnerNameAsync(tenantId, ownerName, condominiumId, year, currentUser, ct);

            var data = new
            {
                owner_name = ownerName,
                year,
                fee_count = fees.Count,
                total_due = fees.Sum(f => f.AmountDue),
                total_paid = fees.Sum(f => f.AmountPaid),
                total_balance = fees.Sum(f => f.Balance),
                fees = fees.Select(f => new
                {
                    installment_number = f.Installment?.InstallmentNumber,
                    due_date = f.Installment?.DueDate.ToString("yyyy-MM-dd"),
                    amount_due = f.AmountDue,
                    amount_paid = f.AmountPaid,
                    balance = f.Balance,
                    status = f.PaymentStatus,
                    unit = f.Unit?.InternalNumber
                })
            };
            return AiToolResult.Ok("get_condomino_payments", data);
        }

        private async Task<AiToolResult> GetOverdueFees(JsonElement input, IUser currentUser, CancellationToken ct)
        {
            var condominiumId = GetInt(input, "condominium_id");
            if (!condominiumId.HasValue)
                return AiToolResult.Error("get_overdue_fees", "Parametro condominium_id mancante.");

            var fees = await _feeService.GetOverdueFeesAsync(condominiumId.Value, currentUser, ct);

            var data = new
            {
                condominium_id = condominiumId.Value,
                overdue_count = fees.Count,
                total_overdue_balance = fees.Sum(f => f.Balance),
                items = fees.Select(f => new
                {
                    user_id = f.UserId,
                    unit = f.Unit?.InternalNumber,
                    installment_number = f.Installment?.InstallmentNumber,
                    due_date = f.Installment?.DueDate.ToString("yyyy-MM-dd"),
                    balance = f.Balance,
                    status = f.PaymentStatus
                })
            };
            return AiToolResult.Ok("get_overdue_fees", data);
        }

        private async Task<AiToolResult> GetInstallmentStatus(JsonElement input, IUser currentUser, CancellationToken ct)
        {
            var condominiumId = GetInt(input, "condominium_id");
            if (!condominiumId.HasValue)
                return AiToolResult.Error("get_installment_status", "Parametro condominium_id mancante.");

            var onlyOverdue = GetBool(input, "only_overdue");

            var installments = onlyOverdue
                ? await _installmentService.GetOverdueInstallmentsAsync(condominiumId.Value, currentUser, ct)
                : await _installmentService.GetOpenInstallmentsAsync(condominiumId.Value, currentUser, ct);

            var data = new
            {
                condominium_id = condominiumId.Value,
                only_overdue = onlyOverdue,
                installment_count = installments.Count,
                items = installments.Select(i => new
                {
                    installment_number = i.InstallmentNumber,
                    due_date = i.DueDate.ToString("yyyy-MM-dd"),
                    total_amount = i.TotalAmount,
                    status = i.Status?.Name,
                    fiscal_year = i.FiscalYear?.StartDate.Year
                })
            };
            return AiToolResult.Ok("get_installment_status", data);
        }

        private async Task<AiToolResult> GetExpenseSummary(
            JsonElement input, Guid tenantId, int? defaultYear, IUser currentUser, CancellationToken ct)
        {
            var condominiumId = GetInt(input, "condominium_id");
            if (!condominiumId.HasValue)
                return AiToolResult.Error("get_expense_summary", "Parametro condominium_id mancante.");

            var year = GetInt(input, "year") ?? defaultYear ?? DateTime.Now.Year;
            var summary = await _expenseService.GetExpenseSummaryAsync(tenantId, condominiumId.Value, year, currentUser, ct);

            return AiToolResult.Ok("get_expense_summary", new
            {
                condominium_id = summary.CondominiumId,
                year = summary.Year,
                document_count = summary.DocumentCount,
                total_gross = summary.TotalGrossAmount,
                unpaid_gross = summary.UnpaidGrossAmount,
                unpaid_count = summary.UnpaidCount
            });
        }

        private async Task<AiToolResult> GetBudgetVsActual(
            JsonElement input, Guid tenantId, int? defaultYear, IUser currentUser, CancellationToken ct)
        {
            var condominiumId = GetInt(input, "condominium_id");
            if (!condominiumId.HasValue)
                return AiToolResult.Error("get_budget_vs_actual", "Parametro condominium_id mancante.");

            var year = GetInt(input, "year") ?? defaultYear ?? DateTime.Now.Year;

            var preventivo = await _budgetService.GetByYearAndTypeAsync(condominiumId.Value, year, BudgetType.Preventivo, currentUser, ct);
            var actual = await _expenseService.GetExpenseSummaryAsync(tenantId, condominiumId.Value, year, currentUser, ct);

            var budgeted = preventivo?.TotalExpenses ?? 0m;
            var spent = actual.TotalGrossAmount;

            return AiToolResult.Ok("get_budget_vs_actual", new
            {
                condominium_id = condominiumId.Value,
                year,
                budget_found = preventivo != null,
                budgeted_expenses = budgeted,
                actual_expenses = spent,
                variance = spent - budgeted,
                over_budget = spent > budgeted
            });
        }

        private async Task<AiToolResult> GetCondominiumList(Guid tenantId, IUser currentUser, CancellationToken ct)
        {
            var condominiums = await _condominiumService.GetActiveCondominiumsAsync(tenantId, currentUser, ct);
            return AiToolResult.Ok("get_condominium_list", new
            {
                count = condominiums.Count,
                items = condominiums.Select(c => new { id = c.Id, name = c.Name })
            });
        }

        private async Task<AiToolResult> GetOwnerBalance(JsonElement input, Guid tenantId, IUser currentUser, CancellationToken ct)
        {
            var ownerName = GetString(input, "owner_name");
            if (string.IsNullOrWhiteSpace(ownerName))
                return AiToolResult.Error("get_owner_balance", "Parametro owner_name mancante.");

            var condominiumId = GetInt(input, "condominium_id");
            var balance = await _feeService.GetTotalBalanceByOwnerAsync(tenantId, ownerName, condominiumId, currentUser, ct);

            return AiToolResult.Ok("get_owner_balance", new
            {
                owner_name = ownerName,
                condominium_id = condominiumId,
                total_balance = balance
            });
        }
    }
}
