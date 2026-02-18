using DomuWave.Domain.Models;
using DomuWave.Services.Interfaces;
using DomuWave.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace DomuWave.Tests.Services;

// ════════════════════════════════════════════════════════════════════════════
// CondominiumFee
// ════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Unit test per ICondominiumFeeService.
/// </summary>
public class CondominiumFeeServiceTests : TestBase
{
    private readonly Mock<ICondominiumFeeService> _serviceMock;
    private readonly FakeUser _currentUser;
    private readonly CancellationToken _ct = CancellationToken.None;
    private const int CondominiumId = 1;

    public CondominiumFeeServiceTests()
    {
        _currentUser = FakeUser.Create();
        _serviceMock = MockOf<ICondominiumFeeService>();
    }

    [Fact]
    public async Task GetUnpaidFeesAsync_ReturnsOnlyUnpaidFees()
    {
        var fees = new List<CondominiumFee>
        {
            new() { Id = 1L, PaymentDate = null },
            new() { Id = 2L, PaymentDate = null }
        };
        _serviceMock.Setup(s => s.GetUnpaidFeesAsync(CondominiumId, _currentUser, _ct))
                    .ReturnsAsync(fees);

        var result = await _serviceMock.Object.GetUnpaidFeesAsync(CondominiumId, _currentUser, _ct);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(f => f.PaymentDate == null);
    }

    [Fact]
    public async Task GetOverdueFeesAsync_ReturnsOverdueFees()
    {
        var fees = new List<CondominiumFee> { new() { Id = 3L } };
        _serviceMock.Setup(s => s.GetOverdueFeesAsync(CondominiumId, _currentUser, _ct))
                    .ReturnsAsync(fees);

        var result = await _serviceMock.Object.GetOverdueFeesAsync(CondominiumId, _currentUser, _ct);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetByUnitIdAsync_ReturnsFeesForUnit()
    {
        var fees = new List<CondominiumFee> { new() { Id = 1L } };
        _serviceMock.Setup(s => s.GetByUnitIdAsync(5, _currentUser, _ct)).ReturnsAsync(fees);

        var result = await _serviceMock.Object.GetByUnitIdAsync(5, _currentUser, _ct);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetTotalDueAsync_ReturnsCorrectTotal()
    {
        _serviceMock.Setup(s => s.GetTotalDueAsync(1L, _currentUser, _ct)).ReturnsAsync(1500m);

        var result = await _serviceMock.Object.GetTotalDueAsync(1L, _currentUser, _ct);

        result.Should().Be(1500m);
    }

    [Fact]
    public async Task RecordPaymentAsync_ValidFee_ReturnsTrue()
    {
        _serviceMock.Setup(s => s.RecordPaymentAsync(1L, 500m, DateTime.Today, "Bonifico", 1L, _currentUser, _ct))
                    .ReturnsAsync(true);

        var result = await _serviceMock.Object
            .RecordPaymentAsync(1L, 500m, DateTime.Today, "Bonifico", 1L, _currentUser, _ct);

        result.Should().BeTrue();
        _serviceMock.Verify(s => s.RecordPaymentAsync(1L, 500m, DateTime.Today, "Bonifico", 1L, _currentUser, _ct), Times.Once);
    }

    [Fact]
    public async Task RecordPaymentAsync_NonExistingFee_ReturnsFalse()
    {
        _serviceMock.Setup(s => s.RecordPaymentAsync(999L, It.IsAny<decimal>(), It.IsAny<DateTime>(),
                               It.IsAny<string>(), It.IsAny<long>(), _currentUser, _ct))
                    .ReturnsAsync(false);

        var result = await _serviceMock.Object
            .RecordPaymentAsync(999L, 100m, DateTime.Today, "Contanti", 1L, _currentUser, _ct);

        result.Should().BeFalse();
    }
}

// ════════════════════════════════════════════════════════════════════════════
// CondominiumInstallment
// ════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Unit test per ICondominiumInstallmentService.
/// </summary>
public class CondominiumInstallmentServiceTests : TestBase
{
    private readonly Mock<ICondominiumInstallmentService> _serviceMock;
    private readonly FakeUser _currentUser;
    private readonly CancellationToken _ct = CancellationToken.None;
    private const int CondominiumId = 1;

    public CondominiumInstallmentServiceTests()
    {
        _currentUser = FakeUser.Create();
        _serviceMock = MockOf<ICondominiumInstallmentService>();
    }

    [Fact]
    public async Task GetByCondominiumIdAsync_ReturnsInstallments()
    {
        var installments = new List<CondominiumInstallment>
        {
            new() { Id = 1 },
            new() { Id = 2 }
        };
        _serviceMock.Setup(s => s.GetByCondominiumIdAsync(CondominiumId, _currentUser, _ct))
                    .ReturnsAsync(installments);

        var result = await _serviceMock.Object.GetByCondominiumIdAsync(CondominiumId, _currentUser, _ct);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByYearAndNumberAsync_ReturnsCorrectInstallment()
    {
        var installment = new CondominiumInstallment { Id = 1, Year = 2024, InstallmentNumber = 3 };
        _serviceMock.Setup(s => s.GetByYearAndNumberAsync(CondominiumId, 2024, 3, _currentUser, _ct))
                    .ReturnsAsync(installment);

        var result = await _serviceMock.Object.GetByYearAndNumberAsync(CondominiumId, 2024, 3, _currentUser, _ct);

        result.Should().NotBeNull();
        result!.Year.Should().Be(2024);
        result.InstallmentNumber.Should().Be(3);
    }

    [Fact]
    public async Task GetOpenInstallmentsAsync_ReturnsOpenInstallments()
    {
        var open = new List<CondominiumInstallment> { new() { Id = 1, Status = "Open" } };
        _serviceMock.Setup(s => s.GetOpenInstallmentsAsync(CondominiumId, _currentUser, _ct))
                    .ReturnsAsync(open);

        var result = await _serviceMock.Object.GetOpenInstallmentsAsync(CondominiumId, _currentUser, _ct);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerateInstallmentsAsync_ValidInput_ReturnsTrue()
    {
        _serviceMock.Setup(s => s.GenerateInstallmentsAsync(CondominiumId, 2025, 1, 1L, _currentUser, _ct))
                    .ReturnsAsync(true);

        var result = await _serviceMock.Object.GenerateInstallmentsAsync(CondominiumId, 2025, 1, 1L, _currentUser, _ct);

        result.Should().BeTrue();
        _serviceMock.Verify(s => s.GenerateInstallmentsAsync(CondominiumId, 2025, 1, 1L, _currentUser, _ct), Times.Once);
    }
}
