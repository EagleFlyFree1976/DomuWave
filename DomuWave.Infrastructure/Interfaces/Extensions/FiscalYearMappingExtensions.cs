using DomuWave.Services.Models;
using DomuWave.Services.Dto.Contabilita.FiscalYear;

namespace DomuWave.Services.Interfaces.Extensions
{
    /// <summary>
    /// Extension methods per la conversione di FiscalYear verso i DTO di output.
    /// </summary>
    public static class FiscalYearMappingExtensions
    {
        /// <summary>
        /// Converte un <see cref="FiscalYear"/> nel DTO di lettura completo.
        /// Il campo Summary non viene popolato qui: usare il metodo specifico del service.
        /// </summary>
        public static FiscalYearReadDto ToReadDto(this FiscalYear fy)
        {
            return new FiscalYearReadDto
            {
                Id                   = fy.Id,
                TenantId             = fy.Tenant?.Id      ?? Guid.Empty,
                CondominiumId        = fy.Condominium?.Id ?? 0,
                CondominiumName      = fy.Condominium?.Name ?? string.Empty,
                Code                 = fy.Code,
                Description          = fy.Description,
                StartDate            = fy.StartDate,
                EndDate              = fy.EndDate,
                StatusId             = fy.Status?.Id ?? 0,
                StatusName           = fy.Status?.Name ?? string.Empty,
                IsActive             = fy.IsActive,
                ClosingDate          = fy.ClosingDate,
                ClosedDate           = fy.ClosedDate,
                LockedDate           = fy.LockedDate,
                ClosingNotes              = fy.ClosingNotes,
                PreviousFiscalYearId      = fy.PreviousFiscalYear?.Id,
                PreviousFiscalYearCode    = fy.PreviousFiscalYear?.Code,
                CreationDate              = fy.CreationDate,
                LastUpdateDate       = fy.LastUpdateDate,
                CreatedByFullName    = fy.CreatedByFullName,
                LastUpdatedByFullName = fy.LastUpdatedByFullName
            };
        }

        /// <summary>
        /// Converte un <see cref="FiscalYear"/> nel DTO compatto per liste e dropdown.
        /// </summary>
        public static FiscalYearListItemDto ToListItemDto(this FiscalYear fy)
        {
            return new FiscalYearListItemDto
            {
                Id          = fy.Id,
                Code        = fy.Code,
                Description = fy.Description,
                StartDate   = fy.StartDate,
                EndDate     = fy.EndDate,
                StatusId               = fy.Status?.Id ?? 0,
                StatusName             = fy.Status?.Name ?? string.Empty,
                IsActive               = fy.IsActive,
                PreviousFiscalYearId   = fy.PreviousFiscalYear?.Id,
                PreviousFiscalYearCode = fy.PreviousFiscalYear?.Code,
            };
        }
    }
}
