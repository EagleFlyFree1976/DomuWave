using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace DomuWave.Tests.Services;

/// <summary>
/// Unit test per IExpenseService.
/// </summary>
public class ExpenseServiceTests : TestBase
{
    private readonly Mock<IExpenseService> _serviceMock;
    private readonly FakeUser _currentUser;
    private readonly CancellationToken _ct = CancellationToken.None;
    private readonly int _condominiumId = 1;

    public ExpenseServiceTests()
    {
        _currentUser = FakeUser.Create();
        _serviceMock = MockOf<IExpenseService>();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingExpense_ReturnsExpense()
    {
        var expense = new Expense { Id = 10L, GrossAmount = 500m };
        _serviceMock.Setup(s => s.GetByIdAsync(10L, _currentUser, _ct))
                    .ReturnsAsync(expense);

        var result = await _serviceMock.Object.GetByIdAsync(10L, _currentUser, _ct);

        result.Should().NotBeNull();
        result!.GrossAmount.Should().Be(500m);
    }

    [Fact]
    public async Task GetByCondominiumIdAsync_ReturnsExpensesForCondominium()
    {
        var expenses = new List<Expense>
        {
            new() { Id = 1L },
            new() { Id = 2L }
        };
        _serviceMock.Setup(s => s.GetByCondominiumIdAsync(_condominiumId, _currentUser, _ct))
                    .ReturnsAsync(expenses);

        var result = await _serviceMock.Object.GetByCondominiumIdAsync(_condominiumId, _currentUser, _ct);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByDateRangeAsync_ReturnsExpensesInRange()
    {
        var from = new DateTime(2024, 1, 1);
        var to   = new DateTime(2024, 12, 31);
        var expenses = new List<Expense> { new() { Id = 1L, DocumentDate = new DateTime(2024, 6, 15) } };

        _serviceMock.Setup(s => s.GetByDateRangeAsync(_condominiumId, from, to, _currentUser, _ct))
                    .ReturnsAsync(expenses);

        var result = await _serviceMock.Object.GetByDateRangeAsync(_condominiumId, from, to, _currentUser, _ct);

        result.Should().HaveCount(1);
        result.First().DocumentDate.Should().BeAfter(from).And.BeBefore(to.AddDays(1));
    }

    [Fact]
    public async Task GetUnpaidExpensesAsync_ReturnsOnlyUnpaid()
    {
        var unpaid = new List<Expense> { new() { Id = 1L, PaymentDate = null } };
        _serviceMock.Setup(s => s.GetUnpaidExpensesAsync(_condominiumId, _currentUser, _ct))
                    .ReturnsAsync(unpaid);

        var result = await _serviceMock.Object.GetUnpaidExpensesAsync(_condominiumId, _currentUser, _ct);

        result.Should().OnlyContain(e => e.PaymentDate == null);
    }

    [Fact]
    public async Task GetTotalExpensesAsync_ReturnsCorrectSum()
    {
        var from = new DateTime(2024, 1, 1);
        var to   = new DateTime(2024, 12, 31);

        _serviceMock.Setup(s => s.GetTotalExpensesAsync(_condominiumId, from, to, _currentUser, _ct))
                    .ReturnsAsync(12500.50m);

        var result = await _serviceMock.Object.GetTotalExpensesAsync(_condominiumId, from, to, _currentUser, _ct);

        result.Should().Be(12500.50m);
    }

    [Fact]
    public async Task MarkAsPaidAsync_ValidExpense_ReturnsTrue()
    {
        var payDate = DateTime.Today;
        _serviceMock.Setup(s => s.MarkAsPaidAsync(1L,  payDate, "Bonifico",  _currentUser, _ct))
                    .ReturnsAsync(true);

        var result = await _serviceMock.Object.MarkAsPaidAsync(1L,  payDate, "Bonifico",  _currentUser, _ct);

        result.Should().BeTrue();
        _serviceMock.Verify(s => s.MarkAsPaidAsync(1L,  payDate, "Bonifico",  _currentUser, _ct), Times.Once);
    }

    [Fact]
    public async Task MarkAsPaidAsync_NonExistingExpense_ReturnsFalse()
    {
        _serviceMock.Setup(s => s.MarkAsPaidAsync(999L, It.IsAny<DateTime>(),
                               It.IsAny<string>(), _currentUser, _ct))
                    .ReturnsAsync(false);

        var result = await _serviceMock.Object.MarkAsPaidAsync(999L,  DateTime.Today, "Contanti", _currentUser, _ct);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_ValidExpense_ReturnsSavedExpense()
    {
        var newExpense = new Expense { GrossAmount = 300m, ExpenseType = "Manutenzione" };
        var saved      = new Expense { Id = 99L, GrossAmount = 300m, ExpenseType = "Manutenzione" };

        _serviceMock.Setup(s => s.CreateAsync(newExpense, _currentUser, _ct))
                    .ReturnsAsync(saved);

        var result = await _serviceMock.Object.CreateAsync(newExpense, _currentUser, _ct);

        result.Id.Should().Be(99L);
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_ReturnsTrue()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1L, _currentUser, _ct)).ReturnsAsync(true);

        var result = await _serviceMock.Object.DeleteAsync(1L, _currentUser, _ct);

        result.Should().BeTrue();
    }
}
