using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class ChartOfAccountsMappingExtensions
{
    public static ChartOfAccountsReadDto ToReadDto(this ChartOfAccounts account)
    {
        if (account == null) return null;

        return new ChartOfAccountsReadDto
        {
            Id             = account.Id,
            Code           = account.Code ?? string.Empty,
            Name           = account.Name ?? string.Empty,
            Type           = account.Type ?? string.Empty,
            Category       = account.Category,
            Level          = account.Level,
            IsActive       = account.IsActive,
            ParentAccountId = account.ParentAccount?.Id,
        };
    }
}
