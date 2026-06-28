using CPQ.Core.Extensions;
using DomuWave.Services.Dto.ElectronicInvoice;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class ElectronicInvoiceMappingExtensions
{
    public static ElectronicInvoiceReadDto ToReadDto(this ElectronicInvoice entity)
    {
        if (entity == null) return null;

        var dto = new ElectronicInvoiceReadDto
        {
            CondominiumId   = entity.Condominium?.Id ?? 0,
            CondominiumName = entity.Condominium?.Name,
            SupplierId      = entity.Supplier?.Id,
            SupplierName    = entity.Supplier?.Name ?? entity.SupplierName,
            ExpenseId       = entity.Expense?.Id,
            StatusId        = entity.StatusId,
            StatusName      = StatusName(entity.StatusId),
            SdiIdentifier   = entity.SdiIdentifier,
            InvoiceNumber   = entity.InvoiceNumber,
            InvoiceDate     = entity.InvoiceDate,
            SupplierVat     = entity.SupplierVat,
            SupplierTaxCode = entity.SupplierTaxCode,
            TotalAmount     = entity.TotalAmount,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    // Allineato a ElectronicInvoiceStatusLookup (lato SQL).
    private static string StatusName(int statusId) => statusId switch
    {
        0 => "Nuova",
        1 => "Collegata",
        2 => "Ignorata",
        _ => string.Empty,
    };
}
