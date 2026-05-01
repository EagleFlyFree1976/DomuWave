using CPQ.Core.Extensions;
using DomuWave.Services.Dto.Assembly;
using DomuWave.Services.Dto.AssemblyAgendaItem;
using DomuWave.Services.Dto.AssemblyAttendance;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class AssemblyMappingExtensions
{
    // ── Assembly ─────────────────────────────────────────────────────────────

    public static AssemblyReadDto ToReadDto(this Models.Assembly entity)
    {
        if (entity == null) return null!;
        var dto = new AssemblyReadDto
        {
            CondominiumId    = entity.Condominium?.Id   ?? 0,
            CondominiumName  = entity.Condominium?.Name,
            FiscalYearId     = entity.FiscalYear?.Id,
            FiscalYearName   = entity.FiscalYear?.Name,
            Title            = entity.Name,
            AssemblyTypeId   = entity.AssemblyType?.Id   ?? 0,
            AssemblyTypeName = entity.AssemblyType?.Name,
            StatusId         = entity.Status?.Id         ?? 0,
            StatusName       = entity.Status?.Name,
            ScheduledDate    = entity.ScheduledDate,
            ActualDate       = entity.ActualDate,
            Location         = entity.Location,
            Notes            = entity.Notes,
            MinutesPath      = entity.MinutesPath,
            AgendaItemCount  = entity.AgendaItems?.Count(i => !i.IsDeleted) ?? 0,
            AttendanceCount  = entity.Attendances?.Count(a => !a.IsDeleted) ?? 0,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static Models.Assembly ToEntity(this CreateAssemblyDto dto, Condominium condominium, Tenant tenant,
        AssemblyTypeLookup assemblyType, AssemblyStatusLookup status, FiscalYear? fiscalYear = null)
    {
        if (dto == null) return null!;
        return new Models.Assembly
        {
            Condominium   = condominium,
            Tenant        = tenant,
            FiscalYear    = fiscalYear,
            AssemblyType  = assemblyType,
            Status        = status,
            Name          = dto.Title,
            ScheduledDate = dto.ScheduledDate,
            Location      = dto.Location,
            Notes         = dto.Notes,
        };
    }

    public static void ApplyUpdate(this Models.Assembly entity, UpdateAssemblyDto dto,
        AssemblyTypeLookup? assemblyType = null, FiscalYear? fiscalYear = null)
    {
        if (dto.Title         != null) entity.Name          = dto.Title;
        if (dto.ScheduledDate != null) entity.ScheduledDate = dto.ScheduledDate.Value;
        if (dto.ActualDate    != null) entity.ActualDate    = dto.ActualDate;
        if (dto.Location      != null) entity.Location      = dto.Location;
        if (dto.Notes         != null) entity.Notes         = dto.Notes;
        if (dto.MinutesPath   != null) entity.MinutesPath   = dto.MinutesPath;
        if (assemblyType      != null) entity.AssemblyType  = assemblyType;
        if (fiscalYear        != null) entity.FiscalYear    = fiscalYear;
    }

    // ── AssemblyAgendaItem ────────────────────────────────────────────────────

    public static AssemblyAgendaItemReadDto ToReadDto(this AssemblyAgendaItem entity)
    {
        if (entity == null) return null!;
        var dto = new AssemblyAgendaItemReadDto
        {
            AssemblyId     = entity.Assembly?.Id     ?? 0,
            OrderIndex     = entity.OrderIndex,
            Title          = entity.Name,
            Description    = entity.Description,
            Resolution     = entity.Resolution,
            VoteResultId   = entity.VoteResult?.Id   ?? 0,
            VoteResultName = entity.VoteResult?.Name,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static AssemblyAgendaItem ToEntity(this CreateAssemblyAgendaItemDto dto,
        Models.Assembly assembly, Tenant tenant, AgendaItemVoteResultLookup voteResult)
    {
        if (dto == null) return null!;
        return new AssemblyAgendaItem
        {
            Assembly    = assembly,
            Tenant      = tenant,
            Name        = dto.Title,
            Description = dto.Description,
            OrderIndex  = dto.OrderIndex,
            VoteResult  = voteResult,
        };
    }

    public static void ApplyUpdate(this AssemblyAgendaItem entity, UpdateAssemblyAgendaItemDto dto,
        AgendaItemVoteResultLookup? voteResult = null)
    {
        if (dto.Title       != null) entity.Name        = dto.Title;
        if (dto.Description != null) entity.Description = dto.Description;
        if (dto.Resolution  != null) entity.Resolution  = dto.Resolution;
        if (dto.OrderIndex  != null) entity.OrderIndex  = dto.OrderIndex.Value;
        if (voteResult      != null) entity.VoteResult  = voteResult;
    }

    // ── AssemblyAttendance ────────────────────────────────────────────────────

    public static AssemblyAttendanceReadDto ToReadDto(this AssemblyAttendance entity)
    {
        if (entity == null) return null!;
        var dto = new AssemblyAttendanceReadDto
        {
            AssemblyId         = entity.Assembly?.Id             ?? 0,
            UnitOwnerId        = entity.UnitOwner?.Id            ?? 0,
            OwnerFirstName     = entity.UnitOwner?.FirstName,
            OwnerLastName      = entity.UnitOwner?.LastName,
            UnitInternalNumber = entity.UnitOwner?.Unit?.InternalNumber,
            AttendanceTypeId   = entity.AttendanceType?.Id       ?? 0,
            AttendanceTypeName = entity.AttendanceType?.Name,
            DelegateName       = entity.DelegateName,
            MillesimalValue    = entity.MillesimalValue,
            Notes              = entity.Description,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static AssemblyAttendance ToEntity(this CreateAssemblyAttendanceDto dto,
        Models.Assembly assembly, Tenant tenant, UnitOwner unitOwner, AttendanceTypeLookup attendanceType)
    {
        if (dto == null) return null!;
        return new AssemblyAttendance
        {
            Assembly        = assembly,
            Tenant          = tenant,
            UnitOwner       = unitOwner,
            AttendanceType  = attendanceType,
            DelegateName    = dto.DelegateName,
            MillesimalValue = dto.MillesimalValue,
            Description     = dto.Notes,
        };
    }

    public static void ApplyUpdate(this AssemblyAttendance entity, UpdateAssemblyAttendanceDto dto,
        AttendanceTypeLookup? attendanceType = null)
    {
        if (dto.AttendanceTypeId != null && attendanceType != null) entity.AttendanceType  = attendanceType;
        if (dto.DelegateName     != null) entity.DelegateName    = dto.DelegateName;
        if (dto.MillesimalValue  != null) entity.MillesimalValue = dto.MillesimalValue.Value;
        if (dto.Notes            != null) entity.Description     = dto.Notes;
    }
}
