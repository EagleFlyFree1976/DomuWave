using CPQ.Core.Extensions;
using DomuWave.Services.Dto.SupplierContract;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class SupplierContractMappingExtensions
{
    public static SupplierContractReadDto ToReadDto(this SupplierContract contract)
    {
        if (contract == null) return null;

        var dto = new SupplierContractReadDto
        {
            CondominiumId   = contract.Condominium?.Id ?? 0,
            CondominiumName = contract.Condominium?.Name,
            SupplierId      = contract.Supplier?.Id ?? 0,
            SupplierName    = contract.Supplier?.CompanyName,
            ContractNumber  = contract.ContractNumber,
            Subject         = contract.Subject,
            StartDate       = contract.StartDate,
            EndDate         = contract.EndDate,
            AnnualAmount    = contract.AnnualAmount,
            Frequency       = contract.Frequency,
            AutoRenewal     = contract.AutoRenewal,
            Status          = contract.Status,
            DocumentPath    = contract.DocumentPath,
            Notes           = contract.Notes,
        };

        dto.SetTraceInfo(contract);
        return dto;
    }

    public static SupplierContract ToEntity(this CreateSupplierContractDto dto, Condominium condominium, Supplier supplier)
    {
        if (dto == null) return null;

        return new SupplierContract
        {
            Tenant         = condominium.Tenant,
            Condominium    = condominium,
            Supplier       = supplier,
            Name           = dto.Subject,
            ContractNumber = dto.ContractNumber,
            Subject        = dto.Subject,
            StartDate      = dto.StartDate,
            EndDate        = dto.EndDate,
            AnnualAmount   = dto.AnnualAmount,
            Frequency      = dto.Frequency,
            AutoRenewal    = dto.AutoRenewal,
            Status         = dto.Status ?? "Active",
            DocumentPath   = dto.DocumentPath,
            Notes          = dto.Notes,
        };
    }

    public static void ApplyUpdate(this SupplierContract entity, UpdateSupplierContractDto dto)
    {
        entity.ContractNumber = dto.ContractNumber;
        entity.Subject        = dto.Subject;
        entity.StartDate      = dto.StartDate;
        entity.EndDate        = dto.EndDate;
        entity.AnnualAmount   = dto.AnnualAmount;
        entity.Frequency      = dto.Frequency;
        entity.AutoRenewal    = dto.AutoRenewal;
        entity.Status         = dto.Status;
        entity.DocumentPath   = dto.DocumentPath;
        entity.Notes          = dto.Notes;
    }
}
