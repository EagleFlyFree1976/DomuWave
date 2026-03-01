using CPQ.Core.Extensions;
using DomuWave.Services.Dto.Supplier;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class SupplierMappingExtensions
{
    public static SupplierReadDto ToReadDto(this Supplier supplier)
    {
        if (supplier == null) return null;

        var dto = new SupplierReadDto
        {
            CompanyName   = supplier.CompanyName,
            VatNumber     = supplier.VatNumber,
            TaxCode       = supplier.TaxCode,
            Address       = supplier.Address,
            City          = supplier.City,
            Province      = supplier.Province,
            PostalCode    = supplier.PostalCode,
            Email         = supplier.Email,
            Phone         = supplier.Phone,
            Pec           = supplier.Pec,
            ContactPerson = supplier.ContactPerson,
            SupplierType  = supplier.SupplierType,
            PaymentTerms  = supplier.PaymentTerms,
            IbanAccount   = supplier.IbanAccount,
            Notes         = supplier.Notes,
            IsActive      = supplier.IsActive,
        };

        dto.SetTraceInfo(supplier);
        return dto;
    }

    public static Supplier ToEntity(this CreateSupplierDto dto, Tenant tenant)
    {
        if (dto == null) return null;

        return new Supplier
        {
            Tenant        = tenant,
            Name          = dto.CompanyName,
            CompanyName   = dto.CompanyName,
            VatNumber     = dto.VatNumber,
            TaxCode       = dto.TaxCode,
            Address       = dto.Address,
            City          = dto.City,
            Province      = dto.Province,
            PostalCode    = dto.PostalCode,
            Email         = dto.Email,
            Phone         = dto.Phone,
            Pec           = dto.Pec,
            ContactPerson = dto.ContactPerson,
            SupplierType  = dto.SupplierType,
            PaymentTerms  = dto.PaymentTerms,
            IbanAccount   = dto.IbanAccount,
            Description   = dto.Notes,
            IsActive      = dto.IsActive,
        };
    }

    public static void ApplyUpdate(this Supplier entity, UpdateSupplierDto dto)
    {
        entity.Name          = dto.CompanyName;
        entity.CompanyName   = dto.CompanyName;
        entity.VatNumber     = dto.VatNumber;
        entity.TaxCode       = dto.TaxCode;
        entity.Address       = dto.Address;
        entity.City          = dto.City;
        entity.Province      = dto.Province;
        entity.PostalCode    = dto.PostalCode;
        entity.Email         = dto.Email;
        entity.Phone         = dto.Phone;
        entity.Pec           = dto.Pec;
        entity.ContactPerson = dto.ContactPerson;
        entity.SupplierType  = dto.SupplierType;
        entity.PaymentTerms  = dto.PaymentTerms;
        entity.IbanAccount   = dto.IbanAccount;
        entity.Description   = dto.Notes;
        entity.IsActive      = dto.IsActive;
    }
}
