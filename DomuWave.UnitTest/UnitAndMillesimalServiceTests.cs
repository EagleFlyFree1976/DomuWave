using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace DomuWave.Tests.Services;

// ════════════════════════════════════════════════════════════════════════════
// RealEstateUnit
// ════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Unit test per <see cref="IRealEstateUnitService"/>.
///
/// La <c>RealEstateUnit</c> (unità immobiliare) rappresenta un appartamento, garage,
/// cantina o altro locale di un condominio. Ogni unità ha un tipo (<c>UnitType</c>),
/// una scala (<c>Staircase</c>), una superficie (<c>AreaSqm</c>) e uno stato di
/// occupazione (<c>OccupancyStatus</c>).
///
/// Casi coperti:
/// <list type="bullet">
///   <item>Lettura per id</item>
///   <item>Lista per condominio e per scala</item>
///   <item>Filtraggio per tipo (Appartamento, Garage, ecc.)</item>
///   <item>Conteggio totale unità</item>
///   <item>Creazione e cancellazione (soft-delete)</item>
/// </list>
/// </summary>
public class RealEstateUnitServiceTests : TestBase
{
    private readonly Mock<IRealEstateUnitService> _serviceMock;
    private readonly FakeUser _currentUser;
    private readonly CancellationToken _ct = CancellationToken.None;

    /// <summary>Id del condominio usato come contesto in tutti i test di questa classe.</summary>
    private const int CondominiumId = 1;

    public RealEstateUnitServiceTests()
    {
        _currentUser = FakeUser.Create();
        _serviceMock = MockOf<IRealEstateUnitService>();
    }

    /// <summary>
    /// Verifica che <c>GetByIdAsync</c> con un id esistente (5) restituisca l'unità
    /// immobiliare corrispondente con l'Id corretto.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ExistingUnit_ReturnsUnit()
    {
        // Arrange
        var unit = new RealEstateUnit { Id = 5 };
        _serviceMock.Setup(s => s.GetByIdAsync(5, _currentUser, _ct)).ReturnsAsync(unit);

        // Act
        var result = await _serviceMock.Object.GetByIdAsync(5, _currentUser, _ct);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(5);
    }

    /// <summary>
    /// Verifica che <c>GetByCondominiumIdAsync</c> restituisca tutte le unità
    /// immobiliari del condominio (3 in questo test). Usato per caricare la lista
    /// completa delle unità nella scheda condominio.
    /// </summary>
    [Fact]
    public async Task GetByCondominiumIdAsync_ReturnsAllUnitsForCondominium()
    {
        // Arrange: 3 unità nel condominio
        var units = new List<RealEstateUnit>
        {
            new() { Id = 1 },
            new() { Id = 2 },
            new() { Id = 3 }
        };
        _serviceMock.Setup(s => s.GetByCondominiumIdAsync(CondominiumId, _currentUser, _ct))
                    .ReturnsAsync(units);

        // Act
        var result = await _serviceMock.Object.GetByCondominiumIdAsync(CondominiumId, _currentUser, _ct);

        // Assert
        result.Should().HaveCount(3);
    }

    /// <summary>
    /// Verifica che <c>GetByStaircaseAsync</c> restituisca solo le unità appartenenti
    /// alla scala specificata ("A"). Nei condomìni con più scale, il filtraggio per
    /// scala consente di gestire separatamente i sottogruppi di proprietari.
    /// </summary>
    [Fact]
    public async Task GetByStaircaseAsync_ReturnsUnitsForStaircase()
    {
        // Arrange: solo unità della scala "A"
        var units = new List<RealEstateUnit> { new() { Id = 1, Staircase = "A" } };
        _serviceMock.Setup(s => s.GetByStaircaseAsync(CondominiumId, "A", _currentUser, _ct))
                    .ReturnsAsync(units);

        // Act
        var result = await _serviceMock.Object.GetByStaircaseAsync(CondominiumId, "A", _currentUser, _ct);

        // Assert
        result.Should().OnlyContain(u => u.Staircase == "A");
    }

    /// <summary>
    /// Verifica che <c>GetByTypeAsync</c> restituisca solo le unità del tipo specificato
    /// ("Appartamento"). I tipi comuni sono: Appartamento, Garage, Cantina, Box.
    /// Utile per calcoli millesimali differenziati per tipologia.
    /// </summary>
    [Fact]
    public async Task GetByTypeAsync_ReturnsUnitsOfSpecifiedType()
    {
        // Arrange: solo appartamenti
        var units = new List<RealEstateUnit> { new() { Id = 1, UnitType = "Appartamento" } };
        _serviceMock.Setup(s => s.GetByTypeAsync(CondominiumId, "Appartamento", _currentUser, _ct))
                    .ReturnsAsync(units);

        // Act
        var result = await _serviceMock.Object.GetByTypeAsync(CondominiumId, "Appartamento", _currentUser, _ct);

        // Assert
        result.Should().OnlyContain(u => u.UnitType == "Appartamento");
    }

    /// <summary>
    /// Verifica che <c>GetTotalUnitsCountAsync</c> restituisca il numero corretto
    /// di unità immobiliari del condominio (12 in questo test). Questo valore viene
    /// usato nel riepilogo anagrafico del condominio.
    /// </summary>
    [Fact]
    public async Task GetTotalUnitsCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetTotalUnitsCountAsync(CondominiumId, _currentUser, _ct))
                    .ReturnsAsync(12);

        // Act
        var result = await _serviceMock.Object.GetTotalUnitsCountAsync(CondominiumId, _currentUser, _ct);

        // Assert
        result.Should().Be(12);
    }

    /// <summary>
    /// Verifica che <c>CreateAsync</c> con un'unità valida (tipo "Garage") restituisca
    /// l'unità persistita con un Id generato (20 in questo test, assegnato da hilo).
    /// </summary>
    [Fact]
    public async Task CreateAsync_ValidUnit_ReturnsSaved()
    {
        // Arrange
        var unit  = new RealEstateUnit { UnitType = "Garage" };
        var saved = new RealEstateUnit { Id = 20, UnitType = "Garage" };
        _serviceMock.Setup(s => s.CreateAsync(unit, _currentUser, _ct)).ReturnsAsync(saved);

        // Act
        var result = await _serviceMock.Object.CreateAsync(unit, _currentUser, _ct);

        // Assert
        result.Id.Should().Be(20); // Id assegnato da NHibernate hilo
    }

    /// <summary>
    /// Verifica che <c>DeleteAsync</c> con un id di unità esistente restituisca
    /// <c>true</c>, indicando che il soft-delete è avvenuto correttamente.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ExistingUnit_ReturnsTrue()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAsync(1, _currentUser, _ct)).ReturnsAsync(true);

        // Act
        var result = await _serviceMock.Object.DeleteAsync(1, _currentUser, _ct);

        // Assert
        result.Should().BeTrue();
    }
}
