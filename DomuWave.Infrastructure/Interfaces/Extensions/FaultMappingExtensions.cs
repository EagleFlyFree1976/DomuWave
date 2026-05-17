using CPQ.Core.Extensions;
using DomuWave.Services.Dto.Fault;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class FaultMappingExtensions
{
    public static FaultReadDto ToReadDto(this Fault entity, int messageCount = 0)
    {
        if (entity == null) return null!;
        var dto = new FaultReadDto
        {
            CondominiumId    = entity.Condominium?.Id   ?? 0,
            CondominiumName  = entity.Condominium?.Name ?? string.Empty,
            UnitId           = entity.Unit?.Id,
            UnitDisplayName  = entity.Unit?.Name,
            StatusId         = entity.Status?.Id   ?? 0,
            StatusName       = entity.Status?.Name ?? string.Empty,
            ReporterUserId   = entity.ReporterUserId,
            ReporterFullName = entity.ReporterFullName,
            Title            = entity.Name,
            Description      = entity.Description,
            MessageCount     = messageCount,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static Fault ToEntity(this CreateFaultDto dto, Condominium condominium, RealEstateUnit? unit, FaultStatus status, Tenant tenant, long reporterUserId, string reporterFullName)
    {
        if (dto == null) return null!;
        return new Fault
        {
            Condominium      = condominium,
            Tenant           = tenant,
            Unit             = unit,
            Status           = status,
            ReporterUserId   = reporterUserId,
            ReporterFullName = reporterFullName,
            Name        = dto.Title,
            Description = dto.Description,
        };
    }

    public static FaultMessageReadDto ToReadDto(this FaultMessage entity)
    {
        if (entity == null) return null!;
        var dto = new FaultMessageReadDto
        {
            FaultId        = entity.Fault?.Id ?? 0,
            AuthorUserId   = entity.AuthorUserId,
            AuthorFullName = entity.AuthorFullName,
            Body           = entity.Name,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static FaultMessage ToEntity(this CreateFaultMessageDto dto, Fault fault, Tenant tenant, long authorUserId, string authorFullName)
    {
        if (dto == null) return null!;
        return new FaultMessage
        {
            Fault          = fault,
            Tenant         = tenant,
            AuthorUserId   = authorUserId,
            AuthorFullName = authorFullName,
            Name           = dto.Body,
        };
    }
}
