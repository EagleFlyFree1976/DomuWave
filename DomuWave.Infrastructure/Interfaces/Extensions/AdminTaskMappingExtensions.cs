using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using DomuWave.Services.Dto.AdminTask;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class AdminTaskMappingExtensions
{
    public static AdminTaskReadDto ToReadDto(this AdminTask entity)
    {
        if (entity == null) return null!;

        var dto = new AdminTaskReadDto
        {
            Title              = entity.Title ?? entity.Name,
            Description        = entity.Description,
            PriorityId         = entity.Priority?.Id   ?? 0,
            PriorityName       = entity.Priority?.Name  ?? string.Empty,
            StatusId           = entity.Status?.Id      ?? 0,
            StatusName         = entity.Status?.Name     ?? string.Empty,
            DueDate            = entity.DueDate,
            AssignedToUserId   = entity.AssignedToUserId,
            AssignedToFullName = entity.AssignedToFullName,
            Condominiums       = (entity.Condominiums ?? new List<AdminTaskCondominium>())
                .Where(c => !c.IsDeleted)
                .Select(c => new AdminTaskCondominiumDto
                {
                    CondominiumId   = c.Condominium?.Id   ?? 0,
                    CondominiumName = c.Condominium?.Name ?? string.Empty,
                })
                .ToList(),
        };

        dto.SetTraceInfo(entity);
        return dto;
    }

    public static AdminTask ToEntity(
        this CreateAdminTaskDto dto,
        Tenant tenant,
        AdminTaskPriorityLookup priority,
        AdminTaskStatusLookup status)
    {
        if (dto == null) return null!;

        return new AdminTask
        {
            Tenant             = tenant,
            Name               = dto.Title,
            Title              = dto.Title,
            Description        = dto.Description,
            Priority           = priority,
            Status             = status,
            DueDate            = dto.DueDate,
            AssignedToUserId   = dto.AssignedToUserId,
            AssignedToFullName = dto.AssignedToFullName,
        };
    }

    public static void ApplyUpdate(
        this AdminTask entity,
        UpdateAdminTaskDto dto,
        AdminTaskPriorityLookup priority,
        AdminTaskStatusLookup status)
    {
        entity.Name               = dto.Title;
        entity.Description        = dto.Description;
        entity.Priority           = priority;
        entity.Status             = status;
        entity.DueDate            = dto.DueDate;
        entity.AssignedToUserId   = dto.AssignedToUserId;
        entity.AssignedToFullName = dto.AssignedToFullName;
    }

    /// <summary>
    /// Allinea i condomìni collegati al set di id desiderato (diff add/remove con soft delete).
    /// `condominiumLookup` mappa id → Condominium (già verificati come appartenenti al tenant).
    /// </summary>
    public static void SyncCondominiums(
        this AdminTask entity,
        IReadOnlyCollection<int> desiredIds,
        IReadOnlyDictionary<int, Condominium> condominiumLookup,
        IUser? currentUser)
    {
        entity.Condominiums ??= new List<AdminTaskCondominium>();

        // Rimuovi (soft) i collegamenti non più desiderati
        foreach (var link in entity.Condominiums.Where(l => !l.IsDeleted).ToList())
        {
            var cid = link.Condominium?.Id ?? 0;
            if (!desiredIds.Contains(cid))
            {
                link.IsDeleted = true;
                if (currentUser != null) link.Trace(currentUser);
            }
        }

        // Aggiungi i nuovi (o riattiva quelli soft-deleted)
        var existingActive = entity.Condominiums
            .Where(l => !l.IsDeleted)
            .Select(l => l.Condominium?.Id ?? 0)
            .ToHashSet();

        foreach (var cid in desiredIds.Distinct())
        {
            if (existingActive.Contains(cid)) continue;
            if (!condominiumLookup.TryGetValue(cid, out var condominium)) continue;

            var reactivated = entity.Condominiums
                .FirstOrDefault(l => l.IsDeleted && (l.Condominium?.Id ?? 0) == cid);
            if (reactivated != null)
            {
                reactivated.IsDeleted = false;
                if (currentUser != null) reactivated.Trace(currentUser);
            }
            else
            {
                var link = new AdminTaskCondominium
                {
                    Tenant      = entity.Tenant,
                    Task        = entity,
                    Condominium = condominium,
                };
                if (currentUser != null) link.Trace(currentUser);
                entity.Condominiums.Add(link);
            }
        }
    }
}
