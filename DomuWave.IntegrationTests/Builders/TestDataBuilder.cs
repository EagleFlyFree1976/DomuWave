using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using DomuWave.Services.Dto.RealEstateUnit;
using DomuWave.Services.Models;

namespace DomuWave.IntegrationTests.Builders;

/// <summary>
/// Factory methods for building valid request DTOs used in integration tests.
/// Uses a unique suffix per call (Guid-based) to avoid conflicts between test runs.
/// </summary>
public static class TestDataBuilder
{
    // ── Condominium ───────────────────────────────────────────────────────────

    public static CreateCondominiumDto Condominium(
        string? code = null,
        string? name = null) => new()
    {
        Code                 = code ?? $"TEST-{ShortId()}",
        Name                 = name ?? $"Condominio Test {ShortId()}",
        TaxCode              = "",
        VatNumber            = "",
        Email                = "",
        Phone                = "",
        Pec                  = "",
        Notes                = "",
        InstallmentFrequency = "Monthly",
        InstallmentDueDay    = 1,
        NumberOfUnits        = 1,
        NumberOfStaircases   = 1,
        NumberOfFloors       = 1,
        TotalMillesimal      = 1000m,
        HasElevator          = true,
        NumberOfElevators    = 1,
        HasCentralHeating    = false,
        HasConcierge         = false,
        MandateStartDate     = new DateTime(2026, 4, 5),
        IsActive             = true,
        Address              = new CondominiumAddressDto
        {
            Street       = "",
            StreetNumber = "",
            City         = "",
            Province     = "",
            PostalCode   = "",
            Country      = "",
        },
    };

    public static UpdateCondominiumDto UpdateCondominium(
        CondominiumReadDto existing,
        string? name = null) => new()
    {
        Name                 = name ?? existing.Name,
        Code                 = existing.Code,
        TaxCode              = existing.TaxCode              ?? "",
        VatNumber            = existing.VatNumber            ?? "",
        Email                = existing.Email                ?? "",
        Phone                = existing.Phone                ?? "",
        Pec                  = existing.Pec                  ?? "",
        Notes                = existing.Notes                ?? "",
        InstallmentFrequency = existing.InstallmentFrequency ?? "Monthly",
        InstallmentDueDay    = existing.InstallmentDueDay,
        NumberOfUnits        = existing.NumberOfUnits,
        NumberOfStaircases   = existing.NumberOfStaircases,
        NumberOfFloors       = existing.NumberOfFloors,
        TotalMillesimal      = existing.TotalMillesimal,
        HasElevator          = existing.HasElevator,
        NumberOfElevators    = existing.NumberOfElevators,
        HasCentralHeating    = existing.HasCentralHeating,
        HasConcierge         = existing.HasConcierge,
        MandateStartDate     = existing.MandateStartDate,
        MandateEndDate       = existing.MandateEndDate,
        IsActive             = existing.IsActive,
        Address              = new CondominiumAddressDto
        {
            Street       = existing.Address?.Street       ?? "",
            StreetNumber = existing.Address?.StreetNumber ?? "",
            City         = existing.Address?.City         ?? "",
            Province     = existing.Address?.Province     ?? "",
            PostalCode   = existing.Address?.PostalCode   ?? "",
            Country      = existing.Address?.Country      ?? "",
        },
    };

    // ── RealEstateUnit ────────────────────────────────────────────────────────

    public static CreateRealEstateUnitDto RealEstateUnit(
        int condominiumId,
        string? internalNumber = null) => new()
    {
        CondominiumId    = condominiumId,
        Staircase        = "A",
        Floor            = 1,
        InternalNumber   = internalNumber ?? $"INT-{ShortId()}",
        UnitType         = "Appartamento",
        OccupancyStatus  = "Owned",
        AreaSqm          = 80m,
        Rooms            = 3,
        NumeroAbitanti   = 2,
        IsActive         = true,
    };

    // ── FiscalYear ────────────────────────────────────────────────────────────

    public static FiscalYearCreateDto FiscalYear(
        int condominiumId,
        int year = 0,
        string? code = null)
    {
        var y = year > 0 ? year : DateTime.UtcNow.Year;
        return new FiscalYearCreateDto
        {
            CondominiumId = condominiumId,
            Code          = code ?? $"{y}-{ShortId()}",
            Description   = $"Esercizio {y}",
            StartDate     = new DateTime(y, 1, 1),
            EndDate       = new DateTime(y, 12, 31),
        };
    }

    // ── Budget ────────────────────────────────────────────────────────────────

    public static CreateBudgetDto Budget(
        int condominiumId,
        int fiscalYearId,
        BudgetType type = BudgetType.Preventivo) => new()
    {
        CondominiumId      = condominiumId,
        FiscalYearId       = fiscalYearId,
        Type               = type,
        Notes              = "Budget di test",
        IncreasePercentage = 0,
    };

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Returns a short (8-char) unique string usable in codes and names.</summary>
    public static string ShortId() => Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
}
