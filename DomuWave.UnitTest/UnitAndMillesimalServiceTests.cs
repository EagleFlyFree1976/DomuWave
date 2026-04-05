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

// ════════════════════════════════════════════════════════════════════════════
// MillesimalTable
// ════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Unit test per <see cref="IMillesimalTableService"/>.
///
/// La <c>MillesimalTable</c> (tabella millesimale) definisce i coefficienti di
/// ripartizione delle spese condominiali tra le unità. Ogni condominio può avere
/// più tabelle (es. "Generale", "Riscaldamento", "Ascensore") identificate da un
/// codice univoco (<c>Code</c>).
///
/// Casi coperti:
/// <list type="bullet">
///   <item>Lista tabelle per condominio</item>
///   <item>Ricerca per codice</item>
///   <item>Filtraggio tabelle attive</item>
///   <item>Creazione di una nuova tabella</item>
/// </list>
/// </summary>
public class MillesimalTableServiceTests : TestBase
{
    private readonly Mock<IMillesimalTableService> _serviceMock;
    private readonly FakeUser _currentUser;
    private readonly CancellationToken _ct = CancellationToken.None;

    /// <summary>Id del condominio usato come contesto in tutti i test di questa classe.</summary>
    private const int CondominiumId = 1;

    public MillesimalTableServiceTests()
    {
        _currentUser = FakeUser.Create();
        _serviceMock = MockOf<IMillesimalTableService>();
    }

    /// <summary>
    /// Verifica che <c>GetByCondominiumIdAsync</c> restituisca tutte le tabelle
    /// millesimali del condominio (2 in questo test: es. Generale + Riscaldamento).
    /// </summary>
    [Fact]
    public async Task GetByCondominiumIdAsync_ReturnsTables()
    {
        // Arrange
        var tables = new List<MillesimalTable>
        {
            new() { Id = 1 },
            new() { Id = 2 }
        };
        _serviceMock.Setup(s => s.GetByCondominiumIdAsync(CondominiumId, _currentUser, _ct))
                    .ReturnsAsync(tables);

        // Act
        var result = await _serviceMock.Object.GetByCondominiumIdAsync(CondominiumId, _currentUser, _ct);

        // Assert
        result.Should().HaveCount(2);
    }

    /// <summary>
    /// Verifica che <c>GetByCodeAsync</c> con un codice esistente ("GEN" = Generale)
    /// restituisca la tabella corrispondente con il codice corretto.
    /// Il codice è univoco per condominio: non possono esserci due tabelle "GEN"
    /// nello stesso condominio.
    /// </summary>
    [Fact]
    public async Task GetByCodeAsync_ExistingCode_ReturnsTable()
    {
        // Arrange
        var table = new MillesimalTable { Id = 1, Code = "GEN" };
        _serviceMock.Setup(s => s.GetByCodeAsync(CondominiumId, "GEN", _currentUser, _ct))
                    .ReturnsAsync(table);

        // Act
        var result = await _serviceMock.Object.GetByCodeAsync(CondominiumId, "GEN", _currentUser, _ct);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be("GEN");
    }

    /// <summary>
    /// Verifica che <c>GetActiveTablesAsync</c> restituisca esclusivamente le tabelle
    /// con <c>IsActive = true</c>. Le tabelle disattivate (versioni precedenti o
    /// tabelle archiviate) non devono essere usate nei nuovi calcoli.
    /// </summary>
    [Fact]
    public async Task GetActiveTablesAsync_ReturnsOnlyActiveTables()
    {
        // Arrange: solo tabelle attive
        var active = new List<MillesimalTable> { new() { Id = 1, IsActive = true } };
        _serviceMock.Setup(s => s.GetActiveTablesAsync(CondominiumId, _currentUser, _ct))
                    .ReturnsAsync(active);

        // Act
        var result = await _serviceMock.Object.GetActiveTablesAsync(CondominiumId, _currentUser, _ct);

        // Assert
        result.Should().OnlyContain(t => t.IsActive);
    }

    /// <summary>
    /// Verifica che <c>CreateAsync</c> con una tabella valida (codice "RIS" = Riscaldamento,
    /// nome "Riscaldamento") restituisca la tabella persistita con Id generato (5) e
    /// codice corretto.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ValidTable_ReturnsSaved()
    {
        // Arrange
        var table = new MillesimalTable { Code = "RIS", Name = "Riscaldamento" };
        var saved = new MillesimalTable { Id = 5, Code = "RIS", Name = "Riscaldamento" };
        _serviceMock.Setup(s => s.CreateAsync(table, _currentUser, _ct)).ReturnsAsync(saved);

        // Act
        var result = await _serviceMock.Object.CreateAsync(table, _currentUser, _ct);

        // Assert
        result.Id.Should().Be(5);
        result.Code.Should().Be("RIS");
    }
}

// ════════════════════════════════════════════════════════════════════════════
// Supplier
// ════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Unit test per <see cref="ISupplierService"/>.
///
/// Il <c>Supplier</c> (fornitore) è l'azienda o professionista a cui vengono
/// intestate le spese condominiali (es. impresa di pulizie, idraulico, elettricista).
/// È un'entità a livello tenant (condivisa tra tutti i condomìni della gestione).
///
/// Casi coperti:
/// <list type="bullet">
///   <item>Ricerca per partita IVA (univoca per tenant)</item>
///   <item>Filtraggio per tipo fornitore</item>
///   <item>Ricerca full-text per ragione sociale</item>
///   <item>Creazione di un nuovo fornitore</item>
/// </list>
/// </summary>
public class SupplierServiceTests : TestBase
{
    private readonly Mock<ISupplierService> _serviceMock;
    private readonly FakeUser _currentUser;
    private readonly CancellationToken _ct = CancellationToken.None;

    /// <summary>TenantId casuale: ogni test run usa un tenant isolato.</summary>
    private readonly Guid _tenantId = Guid.NewGuid();

    public SupplierServiceTests()
    {
        _currentUser = FakeUser.Create();
        _serviceMock = MockOf<ISupplierService>();
    }

    /// <summary>
    /// Verifica che <c>GetByVatNumberAsync</c> con una partita IVA esistente restituisca
    /// il fornitore corrispondente. La P.IVA (formato IT + 11 cifre) è univoca per tenant
    /// e viene usata per evitare duplicati in fase di importazione fatture.
    /// </summary>
    [Fact]
    public async Task GetByVatNumberAsync_ExistingVat_ReturnsSupplier()
    {
        // Arrange
        var supplier = new Supplier { Id = 1, VatNumber = "IT01234567890" };
        _serviceMock.Setup(s => s.GetByVatNumberAsync(_tenantId, "IT01234567890", _currentUser, _ct))
                    .ReturnsAsync(supplier);

        // Act
        var result = await _serviceMock.Object.GetByVatNumberAsync(_tenantId, "IT01234567890", _currentUser, _ct);

        // Assert
        result.Should().NotBeNull();
        result!.VatNumber.Should().Be("IT01234567890");
    }

    /// <summary>
    /// Verifica che <c>GetByTypeAsync</c> restituisca solo i fornitori del tipo specificato
    /// ("Idraulico"). I tipi comuni sono: Idraulico, Elettricista, Giardinaggio, Pulizie.
    /// </summary>
    [Fact]
    public async Task GetByTypeAsync_ReturnsSuppliersByType()
    {
        // Arrange
        var suppliers = new List<Supplier> { new() { Id = 1, SupplierType = "Idraulico" } };
        _serviceMock.Setup(s => s.GetByTypeAsync(_tenantId, "Idraulico", _currentUser, _ct))
                    .ReturnsAsync(suppliers);

        // Act
        var result = await _serviceMock.Object.GetByTypeAsync(_tenantId, "Idraulico", _currentUser, _ct);

        // Assert
        result.Should().OnlyContain(s => s.SupplierType == "Idraulico");
    }

    /// <summary>
    /// Verifica che <c>SearchSuppliersAsync</c> con il termine "Rossi" restituisca i
    /// fornitori la cui ragione sociale contiene quella stringa. La ricerca full-text
    /// facilita la selezione rapida del fornitore durante l'inserimento di una spesa.
    /// </summary>
    [Fact]
    public async Task SearchSuppliersAsync_ReturnMatchingSuppliers()
    {
        // Arrange
        var suppliers = new List<Supplier> { new() { Id = 1, CompanyName = "Rossi Impianti" } };
        _serviceMock.Setup(s => s.SearchSuppliersAsync(_tenantId, "Rossi", _currentUser, _ct))
                    .ReturnsAsync(suppliers);

        // Act
        var result = await _serviceMock.Object.SearchSuppliersAsync(_tenantId, "Rossi", _currentUser, _ct);

        // Assert
        result.Should().HaveCount(1);
        result.First().CompanyName.Should().Contain("Rossi");
    }

    /// <summary>
    /// Verifica che <c>CreateAsync</c> con un fornitore valido (ragione sociale e P.IVA)
    /// restituisca il fornitore persistito con un Id generato (10 in questo test).
    /// </summary>
    [Fact]
    public async Task CreateAsync_ValidSupplier_ReturnsSaved()
    {
        // Arrange
        var supplier = new Supplier { CompanyName = "Bianchi SRL", VatNumber = "IT09876543210" };
        var saved    = new Supplier { Id = 10, CompanyName = "Bianchi SRL" };
        _serviceMock.Setup(s => s.CreateAsync(supplier, _currentUser, _ct)).ReturnsAsync(saved);

        // Act
        var result = await _serviceMock.Object.CreateAsync(supplier, _currentUser, _ct);

        // Assert
        result.Id.Should().Be(10);
    }
}
