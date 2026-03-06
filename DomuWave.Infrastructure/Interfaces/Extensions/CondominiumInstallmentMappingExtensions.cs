using CPQ.Core.Extensions;
using DomuWave.Services.Dto.CondominiumInstallment;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class CondominiumInstallmentMappingExtensions
{
    public static CondominiumInstallmentReadDto ToReadDto(this CondominiumInstallment entity)
    {
        if (entity == null) return null;
        var dto = new CondominiumInstallmentReadDto
        {
            CondominiumId     = entity.Condominium?.Id ?? 0,
            BudgetId          = entity.Budget?.Id,
            FiscalYearId      = entity.FiscalYear?.Id,
            InstallmentNumber = entity.InstallmentNumber,
            DueDate           = entity.DueDate,
            TotalAmount       = entity.TotalAmount,
            Status            = entity.Status ?? string.Empty,
            Notes             = entity.Notes,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static CondominiumInstallment ToEntity(this CreateCondominiumInstallmentDto dto,
        Condominium condominium, Budget? budget, FiscalYear? fiscalYear)
    {
        return new CondominiumInstallment
        {
            Tenant            = condominium.Tenant,
            Condominium       = condominium,
            Budget            = budget,
            FiscalYear        = fiscalYear,
            InstallmentNumber = dto.InstallmentNumber,
            DueDate           = dto.DueDate,
            TotalAmount       = dto.TotalAmount,
            Status            = dto.Status ?? "Open",
            Notes             = dto.Notes,
        };
    }

    public static void ApplyUpdate(this CondominiumInstallment entity, UpdateCondominiumInstallmentDto dto)
    {
        entity.InstallmentNumber = dto.InstallmentNumber;
        entity.DueDate           = dto.DueDate;
        entity.TotalAmount       = dto.TotalAmount;
        entity.Status            = dto.Status;
        entity.Notes             = dto.Notes;
    }
}
