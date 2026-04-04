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
        Code              = code ?? $"TEST-{ShortId()}",
        Name              = name ?? $"Condominio Test {ShortId()}",
        TaxCode           = "00000000000",
        Email             = "test@condominio.it",
        Phone             = "0000000000",
        NumberOfUnits     = 4,
        NumberOfStaircases = 1,
        TotalMillesimal   = 1000m,
        IsActive          = true,
        Address           = new CondominiumAddressDto
        {
            Street     = "Via Test",
            CivicNumber = "1",
            City        = "Milano",
            Province    = "MI",
            ZipCode     = "20100",
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
