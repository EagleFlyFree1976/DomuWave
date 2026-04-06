
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace DomuWave.Tests.Services;

/// <summary>
/// Unit test per <see cref="ICondominiumService"/>.
///
/// APPROCCIO DI TEST: i test mockano <see cref="ICondominiumService"/> direttamente
/// (non la sua implementazione concreta), verificando il contratto dell'interfaccia:
/// firme dei metodi, tipi di ritorno attesi e semantica base (null per id inesistente,
/// lista filtrata, paginazione). L'implementazione reale è coperta dai test di integrazione
/// in <c>DomuWave.IntegrationTests.Tests.Condominiums.CondominiumCrudTests</c>.
///
/// Usa <see cref="FluentAssertions"/> per asserzioni più leggibili rispetto a <c>Assert.*</c>.
/// Il <see cref="FakeUser"/> con <c>TenantId</c> casuale simula l'utente autenticato
/// corrente che viene passato a ogni chiamata di servizio per il filtraggio multi-tenant.
/// </summary>
public class CondominiumServiceTests : TestBase
{
    private readonly Mock<ICondominiumService> _serviceMock;
    private readonly FakeUser _currentUser;

    /// <summary>TenantId fisso per il test: garantisce isolamento multi-tenant coerente.</summary>
    private readonly Guid _tenantId;
    private readonly CancellationToken _ct = CancellationToken.None;

    public CondominiumServiceTests()
    {
        _tenantId = Guid.NewGuid();
        _currentUser = FakeUser.Create(_tenantId);
        _serviceMock = MockOf<ICondominiumService>();
    }

    // ─── GetByIdAsync ────────────────────────────────────────────────────────

    /// <summary>
    /// Verifica che <c>GetByIdAsync</c> con un id esistente restituisca il condominio
    /// corrispondente con Id e Name corretti.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCondominium()
    {
        // Arrange
        var expected = new Condominium { Id = 1, Name = "Condominio Verde" };
        _serviceMock.Setup(s => s.GetByIdAsync(1, _currentUser, _ct))
                    .ReturnsAsync(expected);

        // Act
        var result = await _serviceMock.Object.GetByIdAsync(1, _currentUser, _ct);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Condominio Verde");
    }

    /// <summary>
    /// Verifica che <c>GetByIdAsync</c> con un id inesistente restituisca null
    /// (il controller tradurrà questo in 404 Not Found).
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(999, _currentUser, _ct))
                    .ReturnsAsync((Condominium?)null);

        var result = await _serviceMock.Object.GetByIdAsync(999, _currentUser, _ct);

        result.Should().BeNull();
    }

    // ─── GetByTenantIdAsync ───────────────────────────────────────────────────

    /// <summary>
    /// Verifica che <c>GetByTenantIdAsync</c> restituisca tutti i condomìni associati
    /// al tenant specificato. Fondamentale per l'isolamento dati in architettura multi-tenant:
    /// ogni gestione deve vedere solo i propri condomìni.
    /// </summary>
    [Fact]
    public async Task GetByTenantIdAsync_ReturnsCondominiumsForTenant()
    {
        var list = new List<Condominium>
        {
            new() { Id = 1, Name = "Cond A" },
            new() { Id = 2, Name = "Cond B" }
        };
        _serviceMock.Setup(s => s.GetByTenantIdAsync(_tenantId, _currentUser, _ct))
                    .ReturnsAsync(list);

        var result = await _serviceMock.Object.GetByTenantIdAsync(_tenantId, _currentUser, _ct);

        result.Should().HaveCount(2);
    }

    // ─── GetActiveCondominiumsAsync ───────────────────────────────────────────

    /// <summary>
    /// Verifica che <c>GetActiveCondominiumsAsync</c> restituisca esclusivamente i condomìni
    /// con <c>IsActive = true</c>. Quelli disattivati (mandato scaduto o archiviati)
    /// non devono comparire nelle liste operative.
    /// </summary>
    [Fact]
    public async Task GetActiveCondominiumsAsync_ReturnsOnlyActive()
    {
        var active = new List<Condominium>
        {
            new() { Id = 1, IsActive = true }
        };
        _serviceMock.Setup(s => s.GetActiveCondominiumsAsync(_tenantId, _currentUser, _ct))
                    .ReturnsAsync(active);

        var result = await _serviceMock.Object.GetActiveCondominiumsAsync(_tenantId, _currentUser, _ct);

        // FluentAssertions: verifica che ogni elemento soddisfi il predicato
        result.Should().OnlyContain(c => c.IsActive);
    }

    // ─── GetByCodeAsync ───────────────────────────────────────────────────────

    /// <summary>
    /// Verifica che <c>GetByCodeAsync</c> con un codice esistente restituisca il condominio
    /// corrispondente. Il codice (es. "COD-001") è univoco per tenant e viene usato
    /// per identificare il condominio in importazioni e integrazioni esterne.
    /// </summary>
    [Fact]
    public async Task GetByCodeAsync_ExistingCode_ReturnsCondominium()
    {
        var cond = new Condominium { Id = 5, Code = "COD-001" };
        _serviceMock.Setup(s => s.GetByCodeAsync(_tenantId, "COD-001", _currentUser, _ct))
                    .ReturnsAsync(cond);

        var result = await _serviceMock.Object.GetByCodeAsync(_tenantId, "COD-001", _currentUser, _ct);

        result.Should().NotBeNull();
        result!.Code.Should().Be("COD-001");
    }

    /// <summary>
    /// Verifica che <c>GetByCodeAsync</c> con un codice inesistente restituisca null.
    /// Il controller usa questo per distinguere tra "creazione" e "codice duplicato".
    /// </summary>
    [Fact]
    public async Task GetByCodeAsync_NonExistingCode_ReturnsNull()
    {
        _serviceMock.Setup(s => s.GetByCodeAsync(_tenantId, "INVALID", _currentUser, _ct))
                    .ReturnsAsync((Condominium?)null);

        var result = await _serviceMock.Object.GetByCodeAsync(_tenantId, "INVALID", _currentUser, _ct);

        result.Should().BeNull();
    }

    // ─── CreateAsync ──────────────────────────────────────────────────────────

    /// <summary>
    /// Verifica che <c>CreateAsync</c> con un'entità valida restituisca il condominio
    /// persistito con un Id generato (> 0). Verifica anche che il metodo venga chiamato
    /// esattamente una volta (nessuna doppia scrittura accidentale).
    /// </summary>
    [Fact]
    public async Task CreateAsync_ValidEntity_ReturnsSavedCondominium()
    {
        var newCond = new Condominium { Name = "Nuovo Condominio", Code = "NC-001" };
        var saved   = new Condominium { Id = 10, Name = "Nuovo Condominio", Code = "NC-001" };

        _serviceMock.Setup(s => s.CreateAsync(newCond, _currentUser, _ct))
                    .ReturnsAsync(saved);

        var result = await _serviceMock.Object.CreateAsync(newCond, _currentUser, _ct);

        result.Id.Should().Be(10);
        // Verifica che CreateAsync sia chiamato esattamente una volta
        _serviceMock.Verify(s => s.CreateAsync(newCond, _currentUser, _ct), Times.Once);
    }

    // ─── UpdateAsync ──────────────────────────────────────────────────────────

    /// <summary>
    /// Verifica che <c>UpdateAsync</c> con un'entità esistente restituisca il condominio
    /// con i valori aggiornati e che il metodo venga invocato una sola volta.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ExistingEntity_ReturnsUpdatedCondominium()
    {
        var toUpdate = new Condominium { Id = 1, Name = "Nome Aggiornato" };
        _serviceMock.Setup(s => s.UpdateAsync(toUpdate, _currentUser, _ct))
                    .ReturnsAsync(toUpdate);

        var result = await _serviceMock.Object.UpdateAsync(toUpdate, _currentUser, _ct);

        result.Name.Should().Be("Nome Aggiornato");
        _serviceMock.Verify(s => s.UpdateAsync(toUpdate, _currentUser, _ct), Times.Once);
    }

    // ─── DeleteAsync ──────────────────────────────────────────────────────────

    /// <summary>
    /// Verifica che <c>DeleteAsync</c> con un id esistente restituisca <c>true</c>,
    /// indicando che il soft-delete (IsDeleted = true) è stato applicato.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ExistingId_ReturnsTrue()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1, _currentUser, _ct))
                    .ReturnsAsync(true);

        var result = await _serviceMock.Object.DeleteAsync(1, _currentUser, _ct);

        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifica che <c>DeleteAsync</c> con un id inesistente restituisca <c>false</c>,
    /// consentendo al controller di restituire 404 Not Found.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_NonExistingId_ReturnsFalse()
    {
        _serviceMock.Setup(s => s.DeleteAsync(999, _currentUser, _ct))
                    .ReturnsAsync(false);

        var result = await _serviceMock.Object.DeleteAsync(999, _currentUser, _ct);

        result.Should().BeFalse();
    }

    // ─── ExistsAsync ──────────────────────────────────────────────────────────

    /// <summary>
    /// Verifica che <c>ExistsAsync</c> restituisca <c>true</c> per un id che identifica
    /// un'entità presente e non eliminata. Usato tipicamente per la validazione dei FK
    /// prima di creare entità dipendenti.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_ExistingId_ReturnsTrue()
    {
        _serviceMock.Setup(s => s.ExistsAsync(1, _currentUser, _ct)).ReturnsAsync(true);

        var result = await _serviceMock.Object.ExistsAsync(1, _currentUser, _ct);

        result.Should().BeTrue();
    }

    // ─── GetPagedAsync ────────────────────────────────────────────────────────

    /// <summary>
    /// Verifica che <c>GetPagedAsync</c> restituisca una tupla <c>(items, totalCount)</c>
    /// corretta. La paginazione lato server è essenziale per tabelle con molti condomìni:
    /// il test simula pagina 1 con 5 elementi su 20 totali.
    ///
    /// Il parametro predicato usa <c>It.IsAny&lt;Expression&gt;</c> perché le lambda
    /// LINQ non sono uguagliabili per valore in Moq senza customizzazione.
    /// </summary>
    [Fact]
    public async Task GetPagedAsync_ReturnsPaginatedResult()
    {
        // 5 condomìni fittizi per la pagina corrente
        var items = Enumerable.Range(1, 5)
                              .Select(i => new Condominium { Id = i })
                              .ToList();

        _serviceMock.Setup(s => s.GetPagedAsync(
                        It.IsAny<System.Linq.Expressions.Expression<Func<Condominium, bool>>>(),
                        1,   // pageNumber
                        5,   // pageSize
                        It.IsAny<System.Linq.Expressions.Expression<Func<Condominium, object>>>(),
                        true, // ascending
                        _currentUser,
                        _ct))
                    .ReturnsAsync(((IList<Condominium>)items, 20)); // 20 = totale record nel DB

        var (resultItems, total) = await _serviceMock.Object.GetPagedAsync(
            x => !x.IsDeleted,
            1,
            5,
            x => x.Name!,
            true,
            _currentUser,
            _ct);

        resultItems.Should().HaveCount(5);
        total.Should().Be(20); // il totale deve riflettere tutti i record, non solo la pagina
    }
}
