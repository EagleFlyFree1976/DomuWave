using CPQ.Core.Extensions;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

/// <summary>
/// Estensioni di mapping per il dominio Condominium.
/// </summary>
public static class CondominiumMappingExtensions
{
    /// <summary>
    /// Proietta un'entità <see cref="Condominium"/> nel suo DTO di lettura.
    /// </summary>
    public static CondominiumReadDto ToReadDto(this Condominium condominium)
    {
        if (condominium == null) return null;

        var dto = new CondominiumReadDto
        {
            Id                   = condominium.Id,
            Name                 = condominium.Name,
            Code                 = condominium.Code,
            TaxCode              = condominium.TaxCode,
            VatNumber            = condominium.VatNumber,
            Email                = condominium.Email,
            Phone                = condominium.Phone,
            Pec                  = condominium.Pec,
            NumberOfUnits        = condominium.NumberOfUnits,
            NumberOfStaircases   = condominium.NumberOfStaircases,
            NumberOfFloors       = condominium.NumberOfFloors,
            YearOfConstruction   = condominium.YearOfConstruction,
            TotalMillesimal      = condominium.TotalMillesimal,
            HasElevator          = condominium.HasElevator,
            NumberOfElevators    = condominium.NumberOfElevators,
            HasCentralHeating    = condominium.HasCentralHeating,
            HasConcierge         = condominium.HasConcierge,
            CommonAreasSqm       = condominium.CommonAreasSqm,
            MandateStartDate     = condominium.MandateStartDate,
            MandateEndDate       = condominium.MandateEndDate,
            LastAssemblyDate     = condominium.LastAssemblyDate,
            InstallmentFrequency = condominium.InstallmentFrequency,
            InstallmentDueDay    = condominium.InstallmentDueDay,
            IsActive             = condominium.IsActive,
            Notes                = condominium.Description,
            Address              = condominium.Address?.ToAddressDto(),
        };

        dto.SetTraceInfo(condominium);
        return dto;
    }

    /// <summary>
    /// Proietta un'entità <see cref="CondominiumAddress"/> nel suo DTO.
    /// </summary>
    public static CondominiumAddressDto ToAddressDto(this CondominiumAddress address)
    {
        if (address == null) return null;

        return new CondominiumAddressDto
        {
            Street        = address.Street,
            StreetNumber  = address.StreetNumber,
            PostalCode    = address.PostalCode,
            City          = address.City,
            Province      = address.Province,
            Country       = address.Country,
            Latitude      = address.Latitude,
            Longitude     = address.Longitude,
        };
    }

    /// <summary>
    /// Crea una nuova entità <see cref="Condominium"/> a partire dal DTO di creazione.
    /// </summary>
    public static Condominium ToEntity(this CreateCondominiumDto dto, Tenant tenant)
    {
        if (dto == null) return null;

        var entity = new Condominium
        {
            Name                 = dto.Name,
            Code                 = dto.Code,
            TaxCode              = dto.TaxCode,
            VatNumber            = dto.VatNumber,
            Email                = dto.Email,
            Phone                = dto.Phone,
            Pec                  = dto.Pec,
            NumberOfUnits        = dto.NumberOfUnits,
            NumberOfStaircases   = dto.NumberOfStaircases,
            NumberOfFloors       = dto.NumberOfFloors,
            YearOfConstruction   = dto.YearOfConstruction,
            TotalMillesimal      = dto.TotalMillesimal,
            HasElevator          = dto.HasElevator,
            NumberOfElevators    = dto.NumberOfElevators,
            HasCentralHeating    = dto.HasCentralHeating,
            HasConcierge         = dto.HasConcierge,
            CommonAreasSqm       = dto.CommonAreasSqm,
            MandateStartDate     = dto.MandateStartDate,
            MandateEndDate       = dto.MandateEndDate,
            LastAssemblyDate     = dto.LastAssemblyDate,
            InstallmentFrequency = dto.InstallmentFrequency,
            InstallmentDueDay    = dto.InstallmentDueDay,
            Description          = dto.Notes,
            IsActive             = dto.IsActive,
            Tenant               = tenant,
        };

        if (dto.Address != null)
        {
            entity.Address = new CondominiumAddress
            {
                Street       = dto.Address.Street,
                StreetNumber = dto.Address.StreetNumber,
                PostalCode   = dto.Address.PostalCode,
                City         = dto.Address.City,
                Province     = dto.Address.Province,
                Country      = dto.Address.Country,
                Latitude     = dto.Address.Latitude,
                Longitude    = dto.Address.Longitude,
                Tenant       = tenant,
                Condominium  = entity,
            };
        }

        return entity;
    }

    /// <summary>
    /// Applica i campi del DTO di aggiornamento all'entità <see cref="Condominium"/> esistente.
    /// </summary>
    public static void ApplyUpdate(this Condominium entity, UpdateCondominiumDto dto)
    {
        entity.Name                 = dto.Name;
        entity.Code                 = dto.Code;
        entity.TaxCode              = dto.TaxCode;
        entity.VatNumber            = dto.VatNumber;
        entity.Email                = dto.Email;
        entity.Phone                = dto.Phone;
        entity.Pec                  = dto.Pec;
        entity.NumberOfUnits        = dto.NumberOfUnits;
        entity.NumberOfStaircases   = dto.NumberOfStaircases;
        entity.NumberOfFloors       = dto.NumberOfFloors;
        entity.YearOfConstruction   = dto.YearOfConstruction;
        entity.TotalMillesimal      = dto.TotalMillesimal;
        entity.HasElevator          = dto.HasElevator;
        entity.NumberOfElevators    = dto.NumberOfElevators;
        entity.HasCentralHeating    = dto.HasCentralHeating;
        entity.HasConcierge         = dto.HasConcierge;
        entity.CommonAreasSqm       = dto.CommonAreasSqm;
        entity.MandateStartDate     = dto.MandateStartDate;
        entity.MandateEndDate       = dto.MandateEndDate;
        entity.LastAssemblyDate     = dto.LastAssemblyDate;
        entity.InstallmentFrequency = dto.InstallmentFrequency;
        entity.InstallmentDueDay    = dto.InstallmentDueDay;
        entity.Description          = dto.Notes;
        entity.IsActive             = dto.IsActive;

        entity.Address ??= new CondominiumAddress();
        entity.Address.Street       = dto.Address?.Street;
        entity.Address.StreetNumber = dto.Address?.StreetNumber;
        entity.Address.PostalCode   = dto.Address?.PostalCode;
        entity.Address.City         = dto.Address?.City;
        entity.Address.Province     = dto.Address?.Province;
        entity.Address.Country      = dto.Address?.Country;
        entity.Address.Latitude     = dto.Address?.Latitude;
        entity.Address.Longitude    = dto.Address?.Longitude;
    }
}
