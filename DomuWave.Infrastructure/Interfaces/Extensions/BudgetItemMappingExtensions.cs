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
            BudgetId          = item.Budget?.Id ?? 0,
            AccountId         = item.Account?.Id ?? 0,
            // Usa i valori denormalizzati (snapshot al momento della creazione/ricalcolo).
            // Fallback sulla navigation property per le righe precedenti alla migration.
            AccountCode       = item.AccountCode ?? item.Account?.Code,
            AccountName       = item.AccountName ?? item.Account?.Name,
            AccountType       = item.Account?.Type.ToString(),
            AccountLevel      = item.Account?.Level ?? 0,
            ParentAccountId   = item.Account?.ParentAccount?.Id,
            ParentAccountCode = item.Account?.ParentAccount?.Code,
            ParentAccountName = item.Account?.ParentAccount?.Name,
            Description       = item.Description,
            Amount            = item.Amount,
            Notes             = item.Notes,
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
