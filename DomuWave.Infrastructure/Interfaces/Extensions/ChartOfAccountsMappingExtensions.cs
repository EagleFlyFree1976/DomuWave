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
            Id                          = account.Id,
            TenantId                    = account.Tenant?.Id      ?? Guid.Empty,
            CondominiumId               = account.Condominium?.Id ?? 0,
            Code                        = account.Code ?? string.Empty,
            Name                        = account.Name ?? string.Empty,
            Type                        = account.Type,
            CategoryId                  = account.Category?.Id,
            CategoryName                = account.Category?.Name,
            Level                       = account.Level,
            IsActive                    = account.IsActive,
            ParentAccountId             = account.ParentAccount?.Id,
            DefaultMillesimalTableId    = account.DefaultMillesimalTable?.Id,
            DefaultMillesimalTableName  = account.DefaultMillesimalTable?.Name,
            AllocationMethod            = account.AllocationMethod,
            MillesimalPercentage        = account.MillesimalPercentage,
            FloorWeight                 = account.FloorWeight,
            InhabitantsWeight           = account.InhabitantsWeight,
            ChargeabilityTypeId         = account.ChargeabilityType?.Id ?? ChargeabilityType.Owner,
            ChargeabilityTypeName       = account.ChargeabilityType?.Name ?? string.Empty,
        };
    }

    public static ChartOfAccounts ToEntity(
        this CreateChartOfAccountsDto dto,
        Condominium condominium,
        ChargeabilityType chargeabilityType,
        ChartOfAccounts? parentAccount = null,
        ChartOfAccountsCategory? category = null,
        MillesimalTable? defaultMillesimalTable = null)
    {
        return new ChartOfAccounts
        {
            Condominium            = condominium,
            Tenant                 = condominium.Tenant,
            ParentAccount          = parentAccount,
            Code                   = dto.Code.Trim(),
            Name                   = dto.Name.Trim(),
            Description            = dto.Description,
            Type                   = dto.Type,
            Category               = category,
            Level                  = parentAccount != null ? parentAccount.Level + 1 : 1,
            IsActive               = dto.IsActive,
            IsDeleted              = false,
            DefaultMillesimalTable = defaultMillesimalTable,
            AllocationMethod       = dto.AllocationMethod,
            MillesimalPercentage   = dto.AllocationMethod == AllocationMethod.Mixed ? dto.MillesimalPercentage : null,
            FloorWeight            = dto.AllocationMethod == AllocationMethod.Mixed ? dto.FloorWeight : null,
            InhabitantsWeight      = dto.AllocationMethod == AllocationMethod.Mixed ? dto.InhabitantsWeight : null,
            ChargeabilityType      = chargeabilityType,
        };
    }

    public static void ApplyUpdate(this ChartOfAccounts entity, UpdateChartOfAccountsDto dto,
        ChargeabilityType chargeabilityType,
        ChartOfAccountsCategory? category = null,
        MillesimalTable? defaultMillesimalTable = null,
        ChartOfAccounts? parentAccount = null)
    {
        entity.Code                   = dto.Code.Trim();
        entity.Name                   = dto.Name.Trim();
        entity.Type                   = dto.Type;
        entity.ParentAccount          = parentAccount;
        entity.Level                  = parentAccount != null ? parentAccount.Level + 1 : 1;
        entity.Category               = category;
        entity.Description            = dto.Description;
        entity.IsActive               = dto.IsActive;
        entity.DefaultMillesimalTable = defaultMillesimalTable;
        entity.AllocationMethod       = dto.AllocationMethod;
        entity.MillesimalPercentage   = dto.AllocationMethod == AllocationMethod.Mixed ? dto.MillesimalPercentage : null;
        entity.FloorWeight            = dto.AllocationMethod == AllocationMethod.Mixed ? dto.FloorWeight : null;
        entity.InhabitantsWeight      = dto.AllocationMethod == AllocationMethod.Mixed ? dto.InhabitantsWeight : null;
        entity.ChargeabilityType      = chargeabilityType;
    }
}
