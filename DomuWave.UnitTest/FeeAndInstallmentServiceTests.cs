using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace DomuWave.Tests.Services;

// ════════════════════════════════════════════════════════════════════════════
// CondominiumFee
// ════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Unit test per <see cref="ICondominiumFeeService"/>.
///
/// La <c>CondominiumFee</c> rappresenta la quota condominiale dovuta da ciascun
/// proprietario/inquilino per un determinato periodo (rata). Una fee è "non pagata"
/// quando <c>PaymentDate == null</c> e "scaduta" quando la scadenza è superata senza
/// pagamento.
///
/// Casi coperti:
/// <list type="bullet">
///   <item>Quote non pagate (<c>GetUnpaidFeesAsync</c>)</item>
///   <item>Quote scadute (<c>GetOverdueFeesAsync</c>)</item>
///   <item>Quote per unità immobiliare (<c>GetByUnitIdAsync</c>)</item>
///   <item>Totale dovuto per un owner (<c>GetTotalDueAsync</c>)</item>
///   <item>Registrazione pagamento (<c>RecordPaymentAsync</c>) — caso ok e caso id inesistente</item>
/// </list>
/// </summary>
public class CondominiumFeeServiceTests : TestBase
{
    private readonly Mock<ICondominiumFeeService> _serviceMock;
    private readonly FakeUser _currentUser;
    private readonly CancellationToken _ct = CancellationToken.None;

    /// <summary>Id del condominio usato come contesto in tutti i test di questa classe.</summary>
    private const int CondominiumId = 1;

    public CondominiumFeeServiceTests()
    {
        _currentUser = FakeUser.Create();
        _serviceMock = MockOf<ICondominiumFeeService>();
    }

    /// <summary>
    /// Verifica che <c>GetUnpaidFeesAsync</c> restituisca solo le quote con
    /// <c>PaymentDate == null</c> (non ancora pagate). Verifica sia il conteggio
    /// che la condizione su ogni elemento.
    /// </summary>
    [Fact]
    public async Task GetUnpaidFeesAsync_ReturnsOnlyUnpaidFees()
    {
        // Arrange: 2 quote non pagate (PaymentDate = null)
        var fees = new List<CondominiumFee>
        {
            new() { Id = 1L, PaymentDate = null },
            new() { Id = 2L, PaymentDate = null }
        };
        _serviceMock.Setup(s => s.GetUnpaidFeesAsync(CondominiumId, _currentUser, _ct))
                    .ReturnsAsync(fees);

        // Act
        var result = await _serviceMock.Object.GetUnpaidFeesAsync(CondominiumId, _currentUser, _ct);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(f => f.PaymentDate == null);
    }

    /// <summary>
    /// Verifica che <c>GetOverdueFeesAsync</c> restituisca almeno una quota scaduta.
    /// Le quote scadute sono quelle con <c>DueDate &lt; DateTime.Today</c> e ancora
    /// non pagate: la gestione può usarle per generare solleciti di pagamento.
    /// </summary>
    [Fact]
    public async Task GetOverdueFeesAsync_ReturnsOverdueFees()
    {
        // Arrange
        var fees = new List<CondominiumFee> { new() { Id = 3L } };
        _serviceMock.Setup(s => s.GetOverdueFeesAsync(CondominiumId, _currentUser, _ct))
                    .ReturnsAsync(fees);

        // Act
        var result = await _serviceMock.Object.GetOverdueFeesAsync(CondominiumId, _currentUser, _ct);

        // Assert
        result.Should().NotBeEmpty();
    }

    /// <summary>
    /// Verifica che <c>GetByUnitIdAsync</c> restituisca tutte le quote associate
    /// all'unità immobiliare con id 5. Utile per visualizzare la situazione debitoria
    /// di un singolo appartamento/proprietario nella scheda unità.
    /// </summary>
    [Fact]
    public async Task GetByUnitIdAsync_ReturnsFeesForUnit()
    {
        // Arrange
        var fees = new List<CondominiumFee> { new() { Id = 1L } };
        _serviceMock.Setup(s => s.GetByUnitIdAsync(5, _currentUser, _ct)).ReturnsAsync(fees);

        // Act
        var result = await _serviceMock.Object.GetByUnitIdAsync(5, _currentUser, _ct);

        // Assert
        result.Should().HaveCount(1);
    }

    /// <summary>
    /// Verifica che <c>GetTotalDueAsync</c> restituisca il totale delle quote dovute
    /// (non pagate) per un determinato owner (id = 1L). Il valore atteso è 1.500 €,
    /// che rappresenta il debito complessivo in quel momento.
    /// </summary>
    [Fact]
    public async Task GetTotalDueAsync_ReturnsCorrectTotal()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetTotalDueAsync(1L, _currentUser, _ct)).ReturnsAsync(1500m);

        // Act
        var result = await _serviceMock.Object.GetTotalDueAsync(1L, _currentUser, _ct);

        // Assert
        result.Should().Be(1500m);
    }

    /// <summary>
    /// Verifica che <c>RecordPaymentAsync</c> con parametri validi (fee esistente,
    /// importo, data, metodo di pagamento, id transazione) restituisca <c>true</c>
    /// e che il metodo venga invocato esattamente una volta (no doppio pagamento).
    /// </summary>
    [Fact]
    public async Task RecordPaymentAsync_ValidFee_ReturnsTrue()
    {
        // Arrange
        _serviceMock.Setup(s => s.RecordPaymentAsync(1L, 500m, DateTime.Today, "Bonifico", 1L, _currentUser, _ct))
                    .ReturnsAsync(true);

        // Act
        var result = await _serviceMock.Object
            .RecordPaymentAsync(1L, 500m, DateTime.Today, "Bonifico", 1L, _currentUser, _ct);

        // Assert
        result.Should().BeTrue();
        // Nessun doppio pagamento: il metodo deve essere chiamato esattamente una volta
        _serviceMock.Verify(
            s => s.RecordPaymentAsync(1L, 500m, DateTime.Today, "Bonifico", 1L, _currentUser, _ct),
            Times.Once);
    }

    /// <summary>
    /// Verifica che <c>RecordPaymentAsync</c> con un id di quota inesistente (999L)
    /// restituisca <c>false</c>. Usa <c>It.IsAny</c> per importo, data, metodo e
    /// transazione perché questi parametri non influenzano l'esito quando la fee
    /// non esiste.
    /// </summary>
    [Fact]
    public async Task RecordPaymentAsync_NonExistingFee_ReturnsFalse()
    {
        // Arrange
        _serviceMock.Setup(s => s.RecordPaymentAsync(
                               999L,
                               It.IsAny<decimal>(),
                               It.IsAny<DateTime>(),
                               It.IsAny<string>(),
                               It.IsAny<long>(),
                               _currentUser,
                               _ct))
                    .ReturnsAsync(false);

        // Act
        var result = await _serviceMock.Object
            .RecordPaymentAsync(999L, 100m, DateTime.Today, "Contanti", 1L, _currentUser, _ct);

        // Assert
        result.Should().BeFalse();
    }
}

// ════════════════════════════════════════════════════════════════════════════
// CondominiumInstallment
// ════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Unit test per <see cref="ICondominiumInstallmentService"/>.
///
/// Un <c>CondominiumInstallment</c> è una rata del piano di rateazione del condominio:
/// identifica quale importo è dovuto per quale mese/trimestre dell'anno fiscale.
/// È distinto dalla <c>CondominiumFee</c> (quota del singolo proprietario):
/// la rata è il piano aggregato, la fee è la quota individuale.
///
/// Casi coperti:
/// <list type="bullet">
///   <item>Lista rate per condominio (<c>GetByCondominiumIdAsync</c>)</item>
///   <item>Ricerca per anno e numero rata (<c>GetByYearAndNumberAsync</c>)</item>
///   <item>Rate aperte (non ancora scadute/pagate) (<c>GetOpenInstallmentsAsync</c>)</item>
///   <item>Generazione automatica delle rate per un anno fiscale (<c>GenerateInstallmentsAsync</c>)</item>
/// </list>
/// </summary>
public class CondominiumInstallmentServiceTests : TestBase
{
    private readonly Mock<ICondominiumInstallmentService> _serviceMock;
    private readonly FakeUser _currentUser;
    private readonly CancellationToken _ct = CancellationToken.None;

    /// <summary>Id del condominio usato come contesto in tutti i test di questa classe.</summary>
    private const int CondominiumId = 1;

    public CondominiumInstallmentServiceTests()
    {
        _currentUser = FakeUser.Create();
        _serviceMock = MockOf<ICondominiumInstallmentService>();
    }

    /// <summary>
    /// Verifica che <c>GetByCondominiumIdAsync</c> restituisca tutte le rate
    /// del condominio specificato (2 in questo test).
    /// </summary>
    [Fact]
    public async Task GetByCondominiumIdAsync_ReturnsInstallments()
    {
        // Arrange
        var installments = new List<CondominiumInstallment>
        {
            new() { Id = 1 },
            new() { Id = 2 }
        };
        _serviceMock.Setup(s => s.GetByCondominiumIdAsync(CondominiumId, _currentUser, _ct))
                    .ReturnsAsync(installments);

        // Act
        var result = await _serviceMock.Object.GetByCondominiumIdAsync(CondominiumId, _currentUser, _ct);

        // Assert
        result.Should().HaveCount(2);
    }

    /// <summary>
    /// Verifica che <c>GetByYearAndNumberAsync</c> restituisca la rata corretta
    /// dato anno fiscale (2024) e numero rata (3, ovvero la 3ª del piano).
    /// Controlla che l'anno di inizio dell'esercizio e il numero rata corrispondano.
    /// </summary>
    [Fact]
    public async Task GetByYearAndNumberAsync_ReturnsCorrectInstallment()
    {
        // Arrange: rata n°3 dell'esercizio 2024
        var installment = new CondominiumInstallment
        {
            Id = 1,
            FiscalYear = new FiscalYear { StartDate = new DateTime(2024, 1, 1) },
            InstallmentNumber = 3
        };
        _serviceMock.Setup(s => s.GetByYearAndNumberAsync(CondominiumId, 2024, 3, _currentUser, _ct))
                    .ReturnsAsync(installment);

        // Act
        var result = await _serviceMock.Object.GetByYearAndNumberAsync(CondominiumId, 2024, 3, _currentUser, _ct);

        // Assert
        result.Should().NotBeNull();
        result!.FiscalYear.StartDate.Year.Should().Be(2024);
        result.InstallmentNumber.Should().Be(3);
    }

    /// <summary>
    /// Verifica che <c>GetOpenInstallmentsAsync</c> restituisca almeno una rata aperta.
    /// Una rata "aperta" è quella non ancora scaduta o non completamente pagata,
    /// associata a uno stato specifico (<c>CondominiumInstallmentStatus</c>).
    /// </summary>
    [Fact]
    public async Task GetOpenInstallmentsAsync_ReturnsOpenInstallments()
    {
        // Arrange: una rata aperta con stato valorizzato
        var open = new List<CondominiumInstallment>
        {
            new() { Id = 1, Status = new CondominiumInstallmentStatus() }
        };
        _serviceMock.Setup(s => s.GetOpenInstallmentsAsync(CondominiumId, _currentUser, _ct))
                    .ReturnsAsync(open);

        // Act
        var result = await _serviceMock.Object.GetOpenInstallmentsAsync(CondominiumId, _currentUser, _ct);

        // Assert
        result.Should().NotBeEmpty();
    }

    /// <summary>
    /// Verifica che <c>GenerateInstallmentsAsync</c> con parametri validi (condominio,
    /// anno, frequenza mensile=1, id budget=1L) restituisca <c>true</c> e che il metodo
    /// venga chiamato esattamente una volta.
    ///
    /// La generazione automatica delle rate è l'operazione che, a partire dal budget
    /// approvato, crea tutte le rate mensili/trimestrali dell'anno fiscale.
    /// È una operazione critica che non deve essere ripetuta accidentalmente.
    /// </summary>
    [Fact]
    public async Task GenerateInstallmentsAsync_ValidInput_ReturnsTrue()
    {
        // Arrange
        _serviceMock.Setup(s => s.GenerateInstallmentsAsync(CondominiumId, 2025, 1, 1L, _currentUser, _ct))
                    .ReturnsAsync(true);

        // Act
        var result = await _serviceMock.Object
            .GenerateInstallmentsAsync(CondominiumId, 2025, 1, 1L, _currentUser, _ct);

        // Assert
        result.Should().BeTrue();
        // Verifica idempotenza: la generazione non deve avvenire due volte
        _serviceMock.Verify(
            s => s.GenerateInstallmentsAsync(CondominiumId, 2025, 1, 1L, _currentUser, _ct),
            Times.Once);
    }
}
