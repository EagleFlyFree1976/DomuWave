using CPQ.Core.Extensions;
using DomuWave.Services.Dto.ExtraordinaryWork;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class ExtraordinaryWorkMappingExtensions
{
    // ── ExtraordinaryWork ────────────────────────────────────────────

    public static ExtraordinaryWorkReadDto ToReadDto(this ExtraordinaryWork entity)
    {
        if (entity == null) return null;

        var dto = new ExtraordinaryWorkReadDto
        {
            CondominiumId   = entity.Condominium?.Id ?? 0,
            CondominiumName = entity.Condominium?.Name,
            Title           = entity.Title,
            Description     = entity.Description,
            Category        = entity.Category,
            Status          = entity.Status,
            Priority        = entity.Priority,
            RequestedDate   = entity.RequestedDate,
            ApprovedDate    = entity.ApprovedDate,
            StartDate       = entity.StartDate,
            CompletedDate   = entity.CompletedDate,
            ApprovedAmount  = entity.ApprovedAmount,
            ActualCost      = entity.ActualCost,
            Notes           = entity.Notes,
            QuoteCount      = entity.Quotes?.Count(q => !q.IsDeleted) ?? 0,
            ApprovedQuoteId = entity.Quotes?.FirstOrDefault(q => q.IsApproved && !q.IsDeleted)?.Id ?? 0,
        };

        dto.SetTraceInfo(entity);
        return dto;
    }

    public static ExtraordinaryWork ToEntity(this CreateExtraordinaryWorkDto dto, Condominium condominium)
    {
        if (dto == null) return null;

        return new ExtraordinaryWork
        {
            Tenant         = condominium.Tenant,
            Condominium    = condominium,
            Name           = dto.Title,
            Title          = dto.Title,
            Description    = dto.Description,
            Category       = dto.Category,
            Status         = dto.Status ?? "Draft",
            Priority       = dto.Priority ?? "Medium",
            RequestedDate  = dto.RequestedDate,
            ApprovedDate   = dto.ApprovedDate,
            StartDate      = dto.StartDate,
            CompletedDate  = dto.CompletedDate,
            ApprovedAmount = dto.ApprovedAmount,
            ActualCost     = dto.ActualCost,
            Notes          = dto.Notes,
        };
    }

    public static void ApplyUpdate(this ExtraordinaryWork entity, UpdateExtraordinaryWorkDto dto)
    {
        entity.Name          = dto.Title;
        entity.Description   = dto.Description;
        entity.Category      = dto.Category;
        entity.Status        = dto.Status;
        entity.Priority      = dto.Priority;
        entity.RequestedDate = dto.RequestedDate;
        entity.ApprovedDate  = dto.ApprovedDate;
        entity.StartDate     = dto.StartDate;
        entity.CompletedDate = dto.CompletedDate;
        entity.ApprovedAmount = dto.ApprovedAmount;
        entity.ActualCost    = dto.ActualCost;
        entity.Notes         = dto.Notes;
    }

    // ── WorkQuote ────────────────────────────────────────────────────

    public static WorkQuoteReadDto ToReadDto(this WorkQuote entity)
    {
        if (entity == null) return null;

        var dto = new WorkQuoteReadDto
        {
            WorkId       = entity.Work?.Id ?? 0,
            WorkTitle    = entity.Work?.Title,
            SupplierId   = entity.Supplier?.Id,
            SupplierName = entity.Supplier?.CompanyName,
            QuoteNumber  = entity.QuoteNumber,
            QuoteDate    = entity.QuoteDate,
            ValidUntil   = entity.ValidUntil,
            Amount       = entity.Amount,
            Status       = entity.Status,
            IsApproved   = entity.IsApproved,
            Notes        = entity.Notes,
            Documents    = entity.Documents?
                .Where(d => !d.IsDeleted)
                .Select(d => d.ToReadDto())
                .ToList() ?? new List<WorkQuoteDocumentReadDto>(),
        };

        dto.SetTraceInfo(entity);
        return dto;
    }

    public static WorkQuote ToEntity(this CreateWorkQuoteDto dto, ExtraordinaryWork work, Supplier supplier)
    {
        if (dto == null) return null;

        return new WorkQuote
        {
            Tenant      = work.Tenant,
            Work        = work,
            Supplier    = supplier,
            Name        = dto.QuoteNumber ?? $"PV-{DateTime.Now:yyyyMMdd}",
            QuoteNumber = dto.QuoteNumber,
            QuoteDate   = dto.QuoteDate,
            ValidUntil  = dto.ValidUntil,
            Amount      = dto.Amount,
            Status      = dto.Status ?? "Pending",
            Notes       = dto.Notes,
            IsApproved  = false,
        };
    }

    public static void ApplyUpdate(this WorkQuote entity, UpdateWorkQuoteDto dto, Supplier supplier)
    {
        entity.Supplier    = supplier;
        entity.Name        = dto.QuoteNumber ?? entity.Name;
        entity.QuoteNumber = dto.QuoteNumber;
        entity.QuoteDate   = dto.QuoteDate;
        entity.ValidUntil  = dto.ValidUntil;
        entity.Amount      = dto.Amount;
        entity.Status      = dto.Status;
        entity.Notes       = dto.Notes;
    }

    // ── WorkQuoteDocument ────────────────────────────────────────────

    public static WorkQuoteDocumentReadDto ToReadDto(this WorkQuoteDocument entity)
    {
        if (entity == null) return null;

        var dto = new WorkQuoteDocumentReadDto
        {
            QuoteId      = entity.Work?.Id ?? 0,
            Title        = entity.Title,
            FileName     = entity.FileName,
            FilePath     = entity.FilePath,
            FileSize     = entity.FileSize,
            MimeType     = entity.MimeType,
            DocumentType = entity.DocumentType,
            Notes        = entity.Notes,
        };

        dto.SetTraceInfo(entity);
        return dto;
    }

    public static WorkQuoteDocument ToEntity(this CreateWorkQuoteDocumentDto dto, WorkQuote quote)
    {
        if (dto == null) return null;

        return new WorkQuoteDocument
        {
            Tenant       = quote.Tenant,
            Work         = quote,
            Name         = dto.Title,
            Title        = dto.Title,
            FileName     = dto.FileName,
            FilePath     = dto.FilePath,
            FileSize     = dto.FileSize,
            MimeType     = dto.MimeType ?? "application/octet-stream",
            DocumentType = dto.DocumentType,
            Notes        = dto.Notes,
        };
    }
}
