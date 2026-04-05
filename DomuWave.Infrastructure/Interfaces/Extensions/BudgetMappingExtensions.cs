using CPQ.Core.Extensions;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class BudgetMappingExtensions
{
    public static BudgetReadDto ToReadDto(this Budget budget)
    {
        if (budget == null) return null;

        var dto = new BudgetReadDto
        {
            TenantId       = budget.Tenant?.Id     ?? Guid.Empty,
            CondominiumId  = budget.Condominium?.Id ?? 0,
            CondominiumName = budget.Condominium?.Name,
            FiscalYearId   = budget.FiscalYear?.Id ?? 0,
            FiscalYearCode = budget.FiscalYear?.Code,
            Type           = budget.Type,
            ApprovalDate   = budget.ApprovalDate,
            StatusId   = budget.Status?.Id   ?? 0,
            StatusName = budget.Status?.Name ?? string.Empty,
            TotalIncome    = budget.TotalIncome,
            TotalExpenses  = budget.TotalExpenses,
            Notes          = budget.Notes,
        };

        dto.SetTraceInfo(budget);
        return dto;
    }

    public static void ApplyUpdate(this Budget entity, UpdateBudgetDto dto)
    {
        entity.Description = dto.Notes;
        entity.Notes= dto.Notes;
    }
}
