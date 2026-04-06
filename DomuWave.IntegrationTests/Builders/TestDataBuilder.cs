using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using DomuWave.Services.Dto.RealEstateUnit;
using DomuWave.Services.Models;

namespace DomuWave.IntegrationTests.Builders;

/// <summary>
/// Factory statica per costruire DTO di richiesta validi usati nei test di integrazione.
///
/// DESIGN:
/// Ogni metodo genera dati validi con valori predefiniti ragionevoli, permettendo ai test
/// di sovrascrivere solo i parametri rilevanti per lo scenario da verificare.
/// Un suffisso univoco basato su <see cref="Guid"/> viene aggiunto a codici e nomi per
/// garantire l'assenza di conflitti tra esecuzioni parallele o sequenziali sullo stesso DB.
///
/// UTILIZZO TIPICO:
/// <code>
/// var dto = TestDataBuilder.Condominium();                      // dati casuali
/// var dto = TestDataBuilder.Condominium(code: "COD-FISSO");    // codice fisso per test di duplicato
/// var dto = TestDataBuilder.FiscalYear(condominiumId: id);
/// </code>
/// </summary>
public static class TestDataBuilder
{
    // ── Condominium ───────────────────────────────────────────────────────────

    /// <summary>
    /// Crea un <see cref="CreateCondominiumDto"/> valido con tutti i campi obbligatori
    /// valorizzati. Codice e nome sono generati con un suffisso univoco per evitare
    /// conflitti di unicità nel database di test.
    /// </summary>
    /// <param name="code">Codice condominio. Se null, genera "TEST-{ShortId()}".</param>
    /// <param name="name">Nome condominio. Se null, genera "Condominio Test {ShortId()}".</param>
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

    /// <summary>
    /// Crea un <see cref="UpdateCondominiumDto"/> a partire da un condominio esistente
    /// (<paramref name="existing"/>), opzionalmente sovrascrivendo il <paramref name="name"/>.
    /// Mantiene tutti gli altri campi dell'entità originale per evitare perdite accidentali
    /// di dati nell'aggiornamento.
    /// </summary>
    /// <param name="existing">DTO letto dal server prima dell'aggiornamento.</param>
    /// <param name="name">Nuovo nome. Se null, usa il nome esistente.</param>
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

    /// <summary>
    /// Crea un <see cref="CreateRealEstateUnitDto"/> valido per un appartamento di tipo
    /// standard (scala A, piano 1, 80 mq, 3 locali, 2 abitanti).
    /// Il numero interno è generato con suffisso univoco per evitare conflitti di unicità.
    /// </summary>
    /// <param name="condominiumId">Id del condominio di appartenenza (FK obbligatoria).</param>
    /// <param name="internalNumber">Numero interno. Se null, genera "INT-{ShortId()}".</param>
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

    /// <summary>
    /// Crea un <see cref="FiscalYearCreateDto"/> valido per l'anno corrente (o per
    /// <paramref name="year"/> se specificato). Il codice è univoco per evitare
    /// conflitti se il test viene eseguito più volte nello stesso anno.
    /// </summary>
    /// <param name="condominiumId">Id del condominio (FK obbligatoria).</param>
    /// <param name="year">Anno dell'esercizio. Se 0, usa l'anno corrente (<c>DateTime.UtcNow.Year</c>).</param>
    /// <param name="code">Codice esercizio. Se null, genera "{anno}-{ShortId()}".</param>
    public static FiscalYearCreateDto FiscalYear(
        int condominiumId,
        int year = 0,
        string? code = null)
    {
        var y = year > 0 ? year : DateTime.UtcNow.Year;
        var startDate = new DateTime(y, 1, 1);
        return new FiscalYearCreateDto
        {
            CondominiumId = condominiumId,
            Code          = code ?? $"{y}-{ShortId()}",
            Description   = $"Esercizio {y}",
            StartDate     = startDate,
            EndDate       = startDate.AddYears(1).AddDays(-1),
        };
    }

    // ── Budget ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Crea un <see cref="CreateBudgetDto"/> valido di tipo <paramref name="type"/>
    /// (default: <see cref="BudgetType.Preventivo"/>).
    /// Il budget viene creato in stato Draft; per testare il workflow usare i metodi
    /// <c>ApproveBudgetAsync</c> e <c>CloseBudgetAsync</c> nei test.
    /// </summary>
    /// <param name="condominiumId">Id del condominio (FK obbligatoria).</param>
    /// <param name="fiscalYearId">Id dell'esercizio fiscale (FK obbligatoria).</param>
    /// <param name="type">Tipo budget: Preventivo (1) o Consuntivo (2).</param>
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

    /// <summary>
    /// Genera una stringa univoca di 8 caratteri (uppercase) basata su <see cref="Guid.NewGuid()"/>.
    /// Usata come suffisso in codici e nomi per garantire l'assenza di conflitti tra test.
    /// Esempio: "A3F2B7C1".
    /// </summary>
    public static string ShortId() => Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
}
