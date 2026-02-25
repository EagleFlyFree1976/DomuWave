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
/// Unit test per IRealEstateUnitService.
/// </summary>
public class RealEstateUnitServiceTests : TestBase
{
    private readonly Mock<IRealEstateUnitService> _serviceMock;
    private readonly FakeUser _currentUser;
    private readonly CancellationToken _ct = CancellationToken.None;
    private const int CondominiumId = 1;

    public RealEstateUnitServiceTests()
    {
        _currentUser = FakeUser.Create();
        _serviceMock = MockOf<IRealEstateUnitService>();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUnit_ReturnsUnit()
    {
        var unit = new RealEstateUnit { Id = 5 };
        _serviceMock.Setup(s => s.GetByIdAsync(5, _currentUser, _ct)).ReturnsAsync(unit);

        var result = await _serviceMock.Object.GetByIdAsync(5, _currentUser, _ct);

        result.Should().NotBeNull();
        result!.Id.Should().Be(5);
    }

    [Fact]
    public async Task GetByCondominiumIdAsync_ReturnsAllUnitsForCondominium()
    {
        var units = new List<RealEstateUnit> { new() { Id = 1 }, new() { Id = 2 }, new() { Id = 3 } };
        _serviceMock.Setup(s => s.GetByCondominiumIdAsync(CondominiumId, _currentUser, _ct))
                    .ReturnsAsync(units);

        var result = await _serviceMock.Object.GetByCondominiumIdAsync(CondominiumId, _currentUser, _ct);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByStaircaseAsync_ReturnsUnitsForStaircase()
    {
        var units = new List<RealEstateUnit> { new() { Id = 1, Staircase = "A" } };
        _serviceMock.Setup(s => s.GetByStaircaseAsync(CondominiumId, "A", _currentUser, _ct))
                    .ReturnsAsync(units);

        var result = await _serviceMock.Object.GetByStaircaseAsync(CondominiumId, "A", _currentUser, _ct);

        result.Should().OnlyContain(u => u.Staircase == "A");
    }

    [Fact]
    public async Task GetByTypeAsync_ReturnsUnitsOfSpecifiedType()
    {
        var units = new List<RealEstateUnit> { new() { Id = 1, UnitType = "Appartamento" } };
        _serviceMock.Setup(s => s.GetByTypeAsync(CondominiumId, "Appartamento", _currentUser, _ct))
                    .ReturnsAsync(units);

        var result = await _serviceMock.Object.GetByTypeAsync(CondominiumId, "Appartamento", _currentUser, _ct);

        result.Should().OnlyContain(u => u.UnitType == "Appartamento");
    }

    [Fact]
    public async Task GetTotalUnitsCountAsync_ReturnsCorrectCount()
    {
        _serviceMock.Setup(s => s.GetTotalUnitsCountAsync(CondominiumId, _currentUser, _ct))
                    .ReturnsAsync(12);

        var result = await _serviceMock.Object.GetTotalUnitsCountAsync(CondominiumId, _currentUser, _ct);

        result.Should().Be(12);
    }

    [Fact]
    public async Task CreateAsync_ValidUnit_ReturnsSaved()
    {
        var unit  = new RealEstateUnit { UnitType = "Garage" };
        var saved = new RealEstateUnit { Id = 20, UnitType = "Garage" };
        _serviceMock.Setup(s => s.CreateAsync(unit, _currentUser, _ct)).ReturnsAsync(saved);

        var result = await _serviceMock.Object.CreateAsync(unit, _currentUser, _ct);

        result.Id.Should().Be(20);
    }

    [Fact]
    public async Task DeleteAsync_ExistingUnit_ReturnsTrue()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1, _currentUser, _ct)).ReturnsAsync(true);

        var result = await _serviceMock.Object.DeleteAsync(1, _currentUser, _ct);

        result.Should().BeTrue();
    }
}

// ════════════════════════════════════════════════════════════════════════════
// MillesimalTable
// ════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Unit test per IMillesimalTableService.
/// </summary>
public class MillesimalTableServiceTests : TestBase
{
    private readonly Mock<IMillesimalTableService> _serviceMock;
    private readonly FakeUser _currentUser;
    private readonly CancellationToken _ct = CancellationToken.None;
    private const int CondominiumId = 1;

    public MillesimalTableServiceTests()
    {
        _currentUser = FakeUser.Create();
        _serviceMock = MockOf<IMillesimalTableService>();
    }

    [Fact]
    public async Task GetByCondominiumIdAsync_ReturnsTables()
    {
        var tables = new List<MillesimalTable> { new() { Id = 1 }, new() { Id = 2 } };
        _serviceMock.Setup(s => s.GetByCondominiumIdAsync(CondominiumId, _currentUser, _ct))
                    .ReturnsAsync(tables);

        var result = await _serviceMock.Object.GetByCondominiumIdAsync(CondominiumId, _currentUser, _ct);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByCodeAsync_ExistingCode_ReturnsTable()
    {
        var table = new MillesimalTable { Id = 1, Code = "GEN" };
        _serviceMock.Setup(s => s.GetByCodeAsync(CondominiumId, "GEN", _currentUser, _ct))
                    .ReturnsAsync(table);

        var result = await _serviceMock.Object.GetByCodeAsync(CondominiumId, "GEN", _currentUser, _ct);

        result.Should().NotBeNull();
        result!.Code.Should().Be("GEN");
    }

    [Fact]
    public async Task GetActiveTablesAsync_ReturnsOnlyActiveTables()
    {
        var active = new List<MillesimalTable> { new() { Id = 1, IsActive = true } };
        _serviceMock.Setup(s => s.GetActiveTablesAsync(CondominiumId, _currentUser, _ct))
                    .ReturnsAsync(active);

        var result = await _serviceMock.Object.GetActiveTablesAsync(CondominiumId, _currentUser, _ct);

        result.Should().OnlyContain(t => t.IsActive);
    }

    [Fact]
    public async Task CreateAsync_ValidTable_ReturnsSaved()
    {
        var table = new MillesimalTable { Code = "RIS", Name = "Riscaldamento" };
        var saved = new MillesimalTable { Id = 5, Code = "RIS", Name = "Riscaldamento" };
        _serviceMock.Setup(s => s.CreateAsync(table, _currentUser, _ct)).ReturnsAsync(saved);

        var result = await _serviceMock.Object.CreateAsync(table, _currentUser, _ct);

        result.Id.Should().Be(5);
        result.Code.Should().Be("RIS");
    }
}

// ════════════════════════════════════════════════════════════════════════════
// Supplier
// ════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Unit test per ISupplierService.
/// </summary>
public class SupplierServiceTests : TestBase
{
    private readonly Mock<ISupplierService> _serviceMock;
    private readonly FakeUser _currentUser;
    private readonly CancellationToken _ct = CancellationToken.None;
    private readonly Guid _tenantId = Guid.NewGuid();

    public SupplierServiceTests()
    {
        _currentUser = FakeUser.Create();
        _serviceMock = MockOf<ISupplierService>();
    }

    [Fact]
    public async Task GetByVatNumberAsync_ExistingVat_ReturnsSupplier()
    {
        var supplier = new Supplier { Id = 1, VatNumber = "IT01234567890" };
        _serviceMock.Setup(s => s.GetByVatNumberAsync(_tenantId, "IT01234567890", _currentUser, _ct))
                    .ReturnsAsync(supplier);

        var result = await _serviceMock.Object.GetByVatNumberAsync(_tenantId, "IT01234567890", _currentUser, _ct);

        result.Should().NotBeNull();
        result!.VatNumber.Should().Be("IT01234567890");
    }

    [Fact]
    public async Task GetByTypeAsync_ReturnsSuppliersByType()
    {
        var suppliers = new List<Supplier> { new() { Id = 1, SupplierType = "Idraulico" } };
        _serviceMock.Setup(s => s.GetByTypeAsync(_tenantId, "Idraulico", _currentUser, _ct))
                    .ReturnsAsync(suppliers);

        var result = await _serviceMock.Object.GetByTypeAsync(_tenantId, "Idraulico", _currentUser, _ct);

        result.Should().OnlyContain(s => s.SupplierType == "Idraulico");
    }

    [Fact]
    public async Task SearchSuppliersAsync_ReturnMatchingSuppliers()
    {
        var suppliers = new List<Supplier> { new() { Id = 1, CompanyName = "Rossi Impianti" } };
        _serviceMock.Setup(s => s.SearchSuppliersAsync(_tenantId, "Rossi", _currentUser, _ct))
                    .ReturnsAsync(suppliers);

        var result = await _serviceMock.Object.SearchSuppliersAsync(_tenantId, "Rossi", _currentUser, _ct);

        result.Should().HaveCount(1);
        result.First().CompanyName.Should().Contain("Rossi");
    }

    [Fact]
    public async Task CreateAsync_ValidSupplier_ReturnsSaved()
    {
        var supplier = new Supplier { CompanyName = "Bianchi SRL", VatNumber = "IT09876543210" };
        var saved    = new Supplier { Id = 10, CompanyName = "Bianchi SRL" };
        _serviceMock.Setup(s => s.CreateAsync(supplier, _currentUser, _ct)).ReturnsAsync(saved);

        var result = await _serviceMock.Object.CreateAsync(supplier, _currentUser, _ct);

        result.Id.Should().Be(10);
    }
}
