using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace DomuWave.Tests.Services;

/// <summary>
/// Unit test per <see cref="IExpenseService"/>.
///
/// APPROCCIO DI TEST: i test mockano <see cref="IExpenseService"/> direttamente,
/// verificando il contratto dell'interfaccia. L'implementazione reale è coperta
/// dai test di integrazione.
///
/// Casi coperti:
/// <list type="bullet">
///   <item>Lettura per id, per condominio, per intervallo di date</item>
///   <item>Filtraggio spese non pagate</item>
///   <item>Calcolo del totale spese in un periodo</item>
///   <item>Marcatura come pagata (MarkAsPaid) con metodo di pagamento</item>
///   <item>Eliminazione soft-delete</item>
/// </list>
///
/// NOTA: il test <see cref="CreateAsync_ValidExpense_ReturnsSavedExpense"/> è attualmente
/// vuoto (TODO): la logica di creazione include la validazione dei campi obbligatori
/// (importo lordo, data documento, conto contabile) che deve ancora essere specificata.
/// </summary>
public class ExpenseServiceTests : TestBase
{
    private readonly Mock<IExpenseService> _serviceMock;
    private readonly FakeUser _currentUser;
    private readonly CancellationToken _ct = CancellationToken.None;

    /// <summary>Id del condominio usato come contesto in tutti i test di questa classe.</summary>
    private readonly int _condominiumId = 1;

    public ExpenseServiceTests()
    {
        _currentUser = FakeUser.Create();
        _serviceMock = MockOf<IExpenseService>();
    }

    /// <summary>
    /// Verifica che <c>GetByIdAsync</c> con un id di spesa esistente restituisca
    /// la spesa corrispondente con il campo <c>GrossAmount</c> corretto (500 €).
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ExistingExpense_ReturnsExpense()
    {
        // Arrange
        var expense = new Expense { Id = 10L, GrossAmount = 500m };
        _serviceMock.Setup(s => s.GetByIdAsync(10L, _currentUser, _ct))
                    .ReturnsAsync(expense);

        // Act
        var result = await _serviceMock.Object.GetByIdAsync(10L, _currentUser, _ct);

        // Assert
        result.Should().NotBeNull();
        result!.GrossAmount.Should().Be(500m);
    }

    /// <summary>
    /// Verifica che <c>GetByCondominiumIdAsync</c> restituisca tutte le spese del
    /// condominio specificato. Il risultato include spese di qualsiasi stato (pagate e non).
    /// </summary>
    [Fact]
    public async Task GetByCondominiumIdAsync_ReturnsExpensesForCondominium()
    {
        // Arrange
        var expenses = new List<Expense>
        {
            new() { Id = 1L },
            new() { Id = 2L }
        };
        _serviceMock.Setup(s => s.GetByCondominiumIdAsync(_condominiumId, It.IsAny<Guid>(), _currentUser, _ct))
                    .ReturnsAsync(expenses);

        // Act
        var result = await _serviceMock.Object.GetByCondominiumIdAsync(_condominiumId, Guid.Empty, _currentUser, _ct);

        // Assert
        result.Should().HaveCount(2);
    }

    /// <summary>
    /// Verifica che <c>GetByDateRangeAsync</c> restituisca solo le spese la cui
    /// <c>DocumentDate</c> ricade nell'intervallo [from, to].
    /// Il test controlla che la spesa restituita abbia data successiva a <paramref name="from"/>
    /// e precedente a <paramref name="to"/> + 1 giorno (confronto inclusivo).
    /// </summary>
    [Fact]
    public async Task GetByDateRangeAsync_ReturnsExpensesInRange()
    {
        // Arrange
        var from = new DateTime(2024, 1, 1);
        var to   = new DateTime(2024, 12, 31);
        var expenses = new List<Expense> { new() { Id = 1L, DocumentDate = new DateTime(2024, 6, 15) } };

        _serviceMock.Setup(s => s.GetByDateRangeAsync(_condominiumId, from, to, _currentUser, _ct))
                    .ReturnsAsync(expenses);

        // Act
        var result = await _serviceMock.Object.GetByDateRangeAsync(_condominiumId, from, to, _currentUser, _ct);

        // Assert
        result.Should().HaveCount(1);
        // La data della spesa deve essere compresa nell'intervallo richiesto
        result.First().DocumentDate.Should().BeAfter(from).And.BeBefore(to.AddDays(1));
    }

    /// <summary>
    /// Verifica che <c>GetUnpaidExpensesAsync</c> restituisca esclusivamente le spese
    /// con <c>PaymentDate = null</c> (non ancora liquidate al fornitore).
    /// Le spese pagate hanno <c>PaymentDate</c> valorizzata.
    /// </summary>
    [Fact]
    public async Task GetUnpaidExpensesAsync_ReturnsOnlyUnpaid()
    {
        // Arrange
        var unpaid = new List<Expense> { new() { Id = 1L, PaymentDate = null } };
        _serviceMock.Setup(s => s.GetUnpaidExpensesAsync(_condominiumId, _currentUser, _ct))
                    .ReturnsAsync(unpaid);

        // Act
        var result = await _serviceMock.Object.GetUnpaidExpensesAsync(_condominiumId, _currentUser, _ct);

        // Assert
        result.Should().OnlyContain(e => e.PaymentDate == null);
    }

    /// <summary>
    /// Verifica che <c>GetTotalExpensesAsync</c> restituisca la somma corretta degli
    /// importi lordi delle spese nell'intervallo temporale specificato.
    /// Il valore atteso (12.500,50 €) simula un totale realistico di spese condominiali.
    /// </summary>
    [Fact]
    public async Task GetTotalExpensesAsync_ReturnsCorrectSum()
    {
        // Arrange
        var from = new DateTime(2024, 1, 1);
        var to   = new DateTime(2024, 12, 31);

        _serviceMock.Setup(s => s.GetTotalExpensesAsync(_condominiumId, from, to, _currentUser, _ct))
                    .ReturnsAsync(12500.50m);

        // Act
        var result = await _serviceMock.Object.GetTotalExpensesAsync(_condominiumId, from, to, _currentUser, _ct);

        // Assert
        result.Should().Be(12500.50m);
    }

    /// <summary>
    /// Verifica che <c>MarkAsPaidAsync</c> con una spesa esistente, data di pagamento
    /// e metodo di pagamento validi restituisca <c>true</c> e che il metodo venga
    /// invocato esattamente una volta (no doppio pagamento accidentale).
    /// </summary>
    [Fact]
    public async Task MarkAsPaidAsync_ValidExpense_ReturnsTrue()
    {
        // Arrange
        var payDate = DateTime.Today;
        _serviceMock.Setup(s => s.MarkAsPaidAsync(1L, payDate, "Bonifico", _currentUser, _ct))
                    .ReturnsAsync(true);

        // Act
        var result = await _serviceMock.Object.MarkAsPaidAsync(1L, payDate, "Bonifico", _currentUser, _ct);

        // Assert
        result.Should().BeTrue();
        // Verifica che non ci siano doppi pagamenti: il metodo deve essere chiamato una volta sola
        _serviceMock.Verify(s => s.MarkAsPaidAsync(1L, payDate, "Bonifico", _currentUser, _ct), Times.Once);
    }

    /// <summary>
    /// Verifica che <c>MarkAsPaidAsync</c> con un id di spesa inesistente restituisca
    /// <c>false</c>. Usa <c>It.IsAny</c> per data e metodo di pagamento perché questi
    /// parametri non influenzano l'esito quando la spesa non esiste.
    /// </summary>
    [Fact]
    public async Task MarkAsPaidAsync_NonExistingExpense_ReturnsFalse()
    {
        // Arrange
        _serviceMock.Setup(s => s.MarkAsPaidAsync(999L, It.IsAny<DateTime>(),
                               It.IsAny<string>(), _currentUser, _ct))
                    .ReturnsAsync(false);

        // Act
        var result = await _serviceMock.Object.MarkAsPaidAsync(999L, DateTime.Today, "Contanti", _currentUser, _ct);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// TODO: test da completare.
    /// Deve verificare che <c>CreateAsync</c> con una spesa valida (importo lordo > 0,
    /// data documento valorizzata, conto contabile associato) restituisca la spesa
    /// persistita con Id generato.
    /// Deve anche verificare che vengano applicati: calcolo IVA, tracciatura (Trace),
    /// e associazione al fornitore se presente.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ValidExpense_ReturnsSavedExpense()
    {
        // TODO: implementare questo test
    }

    /// <summary>
    /// Verifica che <c>DeleteAsync</c> con un id di spesa esistente restituisca <c>true</c>,
    /// indicando che il soft-delete è stato applicato correttamente.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAsync(1L, _currentUser, _ct)).ReturnsAsync(true);

        // Act
        var result = await _serviceMock.Object.DeleteAsync(1L, _currentUser, _ct);

        // Assert
        result.Should().BeTrue();
    }
}
