using CPQ.Core.Extensions;
using DomuWave.Services.Dto.Consumption;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class ConsumptionMappingExtensions
{
    // ── ConsumptionType ───────────────────────────────────────────────────

    public static ConsumptionTypeReadDto ToReadDto(this ConsumptionType entity)
    {
        if (entity == null) return null;
        var dto = new ConsumptionTypeReadDto
        {
            CondominiumId   = entity.Condominium?.Id ?? 0,
            CondominiumName = entity.Condominium?.Name,
            Name            = entity.Name,
            UnitOfMeasure   = entity.UnitOfMeasure,
            Notes           = entity.Notes,
            IsActive        = entity.IsActive,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static ConsumptionType ToEntity(this CreateConsumptionTypeDto dto, Condominium condominium, Tenant tenant)
    {
        if (dto == null) return null;
        return new ConsumptionType
        {
            Condominium   = condominium,
            Tenant        = tenant,
            Name          = dto.Name,
            UnitOfMeasure = dto.UnitOfMeasure,
            Notes         = dto.Notes,
            IsActive      = true,
        };
    }

    public static void ApplyUpdate(this ConsumptionType entity, UpdateConsumptionTypeDto dto)
    {
        entity.Name          = dto.Name;
        entity.UnitOfMeasure = dto.UnitOfMeasure;
        entity.Notes         = dto.Notes;
        entity.IsActive      = dto.IsActive;
    }

    // ── Meter ─────────────────────────────────────────────────────────────

    public static MeterReadDto ToReadDto(this Meter entity)
    {
        if (entity == null) return null;
        var dto = new MeterReadDto
        {
            ConsumptionTypeId   = entity.ConsumptionType?.Id ?? 0,
            ConsumptionTypeName = entity.ConsumptionType?.Name,
            UnitOfMeasure       = entity.ConsumptionType?.UnitOfMeasure,
            UnitId              = entity.Unit?.Id ?? 0,
            UnitName            = entity.Unit?.DisplayName ?? entity.Unit?.InternalNumber,
            Code                = entity.Code,
            Notes               = entity.Notes,
            IsActive            = entity.IsActive,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static Meter ToEntity(this CreateMeterDto dto, ConsumptionType type, RealEstateUnit unit, Tenant tenant)
    {
        if (dto == null) return null;
        return new Meter
        {
            ConsumptionType = type,
            Unit            = unit,
            Tenant          = tenant,
            Code            = dto.Code,
            Notes           = dto.Notes,
            IsActive        = true,
        };
    }

    public static void ApplyUpdate(this Meter entity, UpdateMeterDto dto)
    {
        entity.Code     = dto.Code;
        entity.Notes    = dto.Notes;
        entity.IsActive = dto.IsActive;
    }

    // ── ConsumptionReading ────────────────────────────────────────────────

    public static ConsumptionReadingReadDto ToReadDto(this ConsumptionReading entity)
    {
        if (entity == null) return null;
        var dto = new ConsumptionReadingReadDto
        {
            MeterId        = entity.Meter?.Id ?? 0,
            MeterCode      = entity.Meter?.Code,
            UnitId         = entity.Meter?.Unit?.Id ?? 0,
            UnitName       = entity.Meter?.Unit?.DisplayName ?? entity.Meter?.Unit?.InternalNumber,
            FiscalYearId   = entity.FiscalYear?.Id ?? 0,
            FiscalYearCode = entity.FiscalYear?.Code,
            InitialDate    = entity.InitialDate,
            InitialValue   = entity.InitialValue,
            FinalDate      = entity.FinalDate,
            FinalValue     = entity.FinalValue,
            Consumption    = entity.Consumption,
            Notes          = entity.Notes,
            ChargeId       = entity.Charge?.Id,
            ChargeStatusId = entity.Charge?.Status?.Id,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static ConsumptionReading ToEntity(this CreateConsumptionReadingDto dto, Meter meter, FiscalYear fiscalYear, Tenant tenant)
    {
        if (dto == null) return null;
        return new ConsumptionReading
        {
            Meter        = meter,
            FiscalYear   = fiscalYear,
            Tenant       = tenant,
            InitialDate  = dto.InitialDate,
            InitialValue = dto.InitialValue,
            FinalDate    = dto.FinalDate,
            FinalValue   = dto.FinalValue,
            Notes        = dto.Notes,
        };
    }

    public static void ApplyUpdate(this ConsumptionReading entity, UpdateConsumptionReadingDto dto)
    {
        entity.InitialDate  = dto.InitialDate;
        entity.InitialValue = dto.InitialValue;
        entity.FinalDate    = dto.FinalDate;
        entity.FinalValue   = dto.FinalValue;
        entity.Notes        = dto.Notes;
    }

    // ── ConsumptionCharge ─────────────────────────────────────────────────

    public static ConsumptionChargeReadDto ToReadDto(this ConsumptionCharge entity)
    {
        if (entity == null) return null;
        var dto = new ConsumptionChargeReadDto
        {
            ConsumptionTypeId   = entity.ConsumptionType?.Id ?? 0,
            ConsumptionTypeName = entity.ConsumptionType?.Name,
            UnitOfMeasure       = entity.ConsumptionType?.UnitOfMeasure,
            FiscalYearId        = entity.FiscalYear?.Id ?? 0,
            FiscalYearCode      = entity.FiscalYear?.Code,
            BudgetId            = entity.Budget?.Id ?? 0,
            ExpenseId           = entity.Expense?.Id,
            TotalAmount         = entity.TotalAmount,
            StatusId            = entity.Status?.Id ?? 0,
            StatusName          = entity.Status?.Name ?? string.Empty,
            Notes               = entity.Notes,
            Items               = entity.Items?
                .Where(i => !i.IsDeleted)
                .Select(i => i.ToItemReadDto())
                .ToList() ?? [],
        };
        dto.HasWarnings = dto.Items.Any(i => i.HasWarning);
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static ConsumptionChargeItemReadDto ToItemReadDto(this ConsumptionChargeItem entity)
    {
        if (entity == null) return null;
        return new ConsumptionChargeItemReadDto
        {
            Id               = entity.Id,
            UnitId           = entity.Unit?.Id ?? 0,
            UnitName         = entity.Unit?.DisplayName ?? entity.Unit?.InternalNumber,
            Consumption      = entity.Consumption,
            TotalConsumption = entity.TotalConsumption,
            Percentage       = entity.Percentage,
            Amount           = entity.Amount,
            HasWarning       = entity.HasWarning,
        };
    }
}
