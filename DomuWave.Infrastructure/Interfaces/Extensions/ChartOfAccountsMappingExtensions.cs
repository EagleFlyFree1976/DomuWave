using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Dto.ChartOfAccounts;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class ChartOfAccountsMappingExtensions
{
    public static ChartOfAccountsReadDto ToReadDto(this ChartOfAccounts account)
    {
        if (account == null) return null;

        return new ChartOfAccountsReadDto
        {
            Id              = account.Id,
            Code            = account.Code ?? string.Empty,
            Name            = account.Name ?? string.Empty,
            Type            = account.Type,
            Category        = account.Category,
            Level           = account.Level,
            IsActive        = account.IsActive,
            ParentAccountId = account.ParentAccount?.Id,
        };
    }

    public static ChartOfAccounts ToEntity(
        this CreateChartOfAccountsDto dto,
        Condominium condominium,
        ChartOfAccounts? parentAccount = null)
    {
        return new ChartOfAccounts
        {
            Condominium   = condominium,
            Tenant        = condominium.Tenant,
            ParentAccount = parentAccount,
            Code          = dto.Code.Trim(),
            Name          = dto.Name.Trim(),
            Description   = dto.Description,
            Type          = dto.Type,
            Category      = dto.Category,
            Level         = parentAccount != null ? parentAccount.Level + 1 : 1,
            IsActive      = dto.IsActive,
            IsDeleted     = false,
        };
    }

    public static void ApplyUpdate(this ChartOfAccounts entity, UpdateChartOfAccountsDto dto)
    {
        entity.Code        = dto.Code.Trim();
        entity.Name        = dto.Name.Trim();
        entity.Type        = dto.Type;
        entity.Category    = dto.Category;
        entity.Description = dto.Description;
        entity.IsActive    = dto.IsActive;
    }
}
