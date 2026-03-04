using CPQ.Core.Extensions;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class BudgetItemMappingExtensions
{
    public static BudgetItemReadDto ToReadDto(this BudgetItem item)
    {
        if (item == null) return null;

        var dto = new BudgetItemReadDto
        {
            BudgetId    = item.Budget?.Id ?? 0,
            AccountId   = item.Account?.Id ?? 0,
            AccountCode = item.Account?.Code,
            AccountName = item.Account?.Name,
            AccountType = item.Account?.Type.ToString(),
            Description = item.Description,
            Amount      = item.Amount,
            Notes       = item.Notes,
        };

        dto.SetTraceInfo(item);
        return dto;
    }

    public static void ApplyUpdate(this BudgetItem entity, UpdateBudgetItemDto dto, ChartOfAccounts account)
    {
        entity.Account = account;
        entity.Name    = dto.Description ?? string.Empty;
        entity.Amount  = dto.Amount;
        entity.Notes   = dto.Notes;
    }
}
