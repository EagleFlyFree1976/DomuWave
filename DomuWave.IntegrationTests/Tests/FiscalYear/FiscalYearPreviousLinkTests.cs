using System.Net;
using DomuWave.IntegrationTests.Builders;
using DomuWave.IntegrationTests.Infrastructure;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using FluentAssertions;
using Xunit;

namespace DomuWave.IntegrationTests.Tests.FiscalYear;

/// <summary>
/// Test di integrazione per la business rule sul collegamento tra esercizi fiscali consecutivi.
///
/// REGOLE DI BUSINESS VERIFICATE:
/// <list type="bullet">
///   <item>Il primo esercizio creato per un condominio ha sempre
///         <c>PreviousFiscalYearId = null</c>: non esiste un esercizio di provenienza.</item>
///   <item>Ogni esercizio successivo, creato con <c>previousFiscalYearId</c> valorizzato,
///         conserva il riferimento all'esercizio di provenienza (<c>PreviousFiscalYearId != null</c>
///         e <c>PreviousFiscalYearCode</c> corrispondente).</item>
///   <item>La data di inizio (<c>StartDate</c>) di un esercizio successivo deve essere
///         uguale al giorno successivo alla <c>EndDate</c> dell'esercizio precedente.</item>
///   <item>Un esercizio creato senza valorizzare <c>previousFiscalYearId</c> (anche se
///         esistono esercizi precedenti) ha <c>PreviousFiscalYearId = null</c>:
///         il collegamento è esplicito, non automatico.</item>
/// </list>
///
/// FLUSSO DEI TEST:
/// <list type="number">
///   <item>Setup (<see cref="InitializeAsync"/>): crea un condominio fresco.</item>
///   <item><see cref="PrimoEsercizio_PreviousFiscalYearId_ENull"/>:
///         verifica che il primo esercizio non abbia predecessore.</item>
///   <item><see cref="SecondoEsercizio_ConPrecedente_ConservaRiferimento"/>:
///         verifica che il secondo esercizio conservi il link e la data di inizio corretta.</item>
///   <item><see cref="CatenaEsercizi_TreEsercizi_LinkCatenaCompleta"/>:
///         verifica una catena di tre esercizi consecutivi.</item>
///   <item><see cref="EsercizioSenzaPrecedenteEsplicito_PreviousFiscalYearId_ENull"/>:
///         verifica che l'omissione esplicita di <c>previousFiscalYearId</c> lasci il campo null.</item>
/// </list>
/// </summary>
[Collection("Integration")]
public class FiscalYearPreviousLinkTests(IntegrationTestFactory factory)
    : IntegrationTestBase(factory), IAsyncLifetime
{
    private int _condominiumId;
    private readonly List<int> _fiscalYearIds = [];

    // ── Lifecycle ──────────────────────────────────────────────────────────────

    /// <summary>Crea un condominio fresco condiviso da tutti i test della classe.</summary>
    public override async Task InitializeAsync()
    {
        var (_, condo) = await PostAsync<CondominiumReadDto>(
            "/api/condominiums", TestDataBuilder.Condominium());
        _condominiumId = condo!.Id;
    }

    /// <summary>
    /// Teardown best-effort: elimina tutti gli esercizi fiscali creati (dal più recente
    /// al più vecchio, per rispettare i vincoli FK), poi il condominio.
    /// </summary>
    public override async Task DisposeAsync()
    {
        // Elimina in ordine inverso di creazione per evitare errori FK
        _fiscalYearIds.Reverse();
        foreach (var id in _fiscalYearIds)
        {
            try { await DeleteAsync($"/api/fiscal-years/{id}"); } catch { }
        }

        try { await DeleteAsync($"/api/condominiums/{_condominiumId}"); } catch { }

        await base.DisposeAsync();
    }

    // ── Test ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Il primo esercizio del condominio non ha un esercizio di provenienza:
    /// <c>PreviousFiscalYearId</c> deve essere <c>null</c> sia nella risposta
    /// alla creazione (201) sia nella GET by-id successiva.
    /// </summary>
    [Fact]
    public async Task PrimoEsercizio_PreviousFiscalYearId_ENull()
    {
        // Arrange: primo esercizio, nessun previousFiscalYearId
        var dto = TestDataBuilder.FiscalYear(_condominiumId, year: 2024);

        // Act
        var (createResponse, created) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", dto);

        // Assert — risposta 201 con link assente
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "la creazione del primo esercizio deve riuscire");
        created.Should().NotBeNull();
        created!.PreviousFiscalYearId.Should().BeNull(
            "il primo esercizio del condominio non ha un esercizio di provenienza");
        created.PreviousFiscalYearCode.Should().BeNullOrEmpty(
            "il primo esercizio del condominio non ha un esercizio di provenienza");

        _fiscalYearIds.Add(created.Id);

        // Verifica anche sulla GET by-id: il campo deve persistere come null
        var fetched = await GetAsync<FiscalYearReadDto>($"/api/fiscal-years/{created.Id}");
        fetched.PreviousFiscalYearId.Should().BeNull(
            "il campo PreviousFiscalYearId deve persistere come null nel database");
        fetched.PreviousFiscalYearCode.Should().BeNullOrEmpty(
            "il campo PreviousFiscalYearCode deve persistere come null nel database");
    }

    /// <summary>
    /// Il secondo esercizio, creato con <c>previousFiscalYearId</c> valorizzato,
    /// deve conservare il riferimento all'esercizio di provenienza e avere
    /// <c>StartDate</c> uguale al giorno successivo alla <c>EndDate</c> del precedente.
    /// </summary>
    [Fact]
    public async Task SecondoEsercizio_ConPrecedente_ConservaRiferimento()
    {
        // Arrange: crea il primo esercizio (2024)
        var (_, primo) = await PostAsync<FiscalYearReadDto>(
            "/api/fiscal-years", TestDataBuilder.FiscalYear(_condominiumId, year: 2024));
        primo.Should().NotBeNull("il primo esercizio deve essere creato con successo");
        _fiscalYearIds.Add(primo!.Id);

        // Calcola la data di inizio attesa: EndDate(2024-12-31) + 1 giorno = 2025-01-01
        var expectedStartDate = primo.EndDate.AddDays(1).Date;

        // Secondo esercizio: StartDate = giorno dopo la fine del primo, EndDate = fine 2025
        var secondoDto = new FiscalYearCreateDto
        {
            CondominiumId        = _condominiumId,
            Code                 = $"2025-{TestDataBuilder.ShortId()}",
            Description          = "Esercizio 2025",
            StartDate            = expectedStartDate,
            EndDate              = new DateTime(2025, 12, 31),
            PreviousFiscalYearId = primo.Id,
        };

        // Act
        var (createResponse, secondo) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", secondoDto);

        // Assert — risposta 201 con link al precedente
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "la creazione del secondo esercizio deve riuscire");
        secondo.Should().NotBeNull();
        _fiscalYearIds.Add(secondo!.Id);

        secondo.PreviousFiscalYearId.Should().Be(primo.Id,
            "il secondo esercizio deve referenziare il primo come esercizio di provenienza");
        secondo.PreviousFiscalYearCode.Should().Be(primo.Code,
            "il codice dell'esercizio di provenienza deve corrispondere al codice del primo esercizio");
        secondo.StartDate.Date.Should().Be(expectedStartDate,
            "la data di inizio del secondo esercizio deve essere il giorno successivo " +
            "alla data di fine del primo esercizio");

        // Verifica persistenza anche sulla GET by-id
        var fetched = await GetAsync<FiscalYearReadDto>($"/api/fiscal-years/{secondo.Id}");
        fetched.PreviousFiscalYearId.Should().Be(primo.Id,
            "il riferimento all'esercizio precedente deve essere persistito correttamente");
        fetched.PreviousFiscalYearCode.Should().Be(primo.Code,
            "il codice dell'esercizio precedente deve essere persistito correttamente");
        fetched.StartDate.Date.Should().Be(expectedStartDate,
            "la StartDate deve essere persistita come giorno successivo alla EndDate del precedente");
    }

    /// <summary>
    /// Catena di tre esercizi consecutivi: verifica che ogni esercizio punti
    /// al precedente e che le date formino una sequenza continua senza buchi.
    ///
    /// Catena attesa:
    /// <code>
    ///   Primo (2023): StartDate=2023-01-01, EndDate=2023-12-31, PreviousFiscalYearId=null
    ///   Secondo(2024): StartDate=2024-01-01, EndDate=2024-12-31, PreviousFiscalYearId=Primo.Id
    ///   Terzo (2025): StartDate=2025-01-01, EndDate=2025-12-31, PreviousFiscalYearId=Secondo.Id
    /// </code>
    /// </summary>
    [Fact]
    public async Task CatenaEsercizi_TreEsercizi_LinkCatenaCompleta()
    {
        // Arrange + Act: crea la catena di tre esercizi

        // Primo esercizio (2023) — nessun predecessore
        var (_, primo) = await PostAsync<FiscalYearReadDto>(
            "/api/fiscal-years", TestDataBuilder.FiscalYear(_condominiumId, year: 2023));
        primo.Should().NotBeNull();
        _fiscalYearIds.Add(primo!.Id);

        // Secondo esercizio (2024) — predecessore = primo
        var (_, secondo) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", new FiscalYearCreateDto
        {
            CondominiumId        = _condominiumId,
            Code                 = $"2024-{TestDataBuilder.ShortId()}",
            Description          = "Esercizio 2024",
            StartDate            = primo.EndDate.AddDays(1),
            EndDate              = new DateTime(2024, 12, 31),
            PreviousFiscalYearId = primo.Id,
        });
        secondo.Should().NotBeNull();
        _fiscalYearIds.Add(secondo!.Id);

        // Terzo esercizio (2025) — predecessore = secondo
        var (_, terzo) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", new FiscalYearCreateDto
        {
            CondominiumId        = _condominiumId,
            Code                 = $"2025-{TestDataBuilder.ShortId()}",
            Description          = "Esercizio 2025",
            StartDate            = secondo.EndDate.AddDays(1),
            EndDate              = new DateTime(2025, 12, 31),
            PreviousFiscalYearId = secondo.Id,
        });
        terzo.Should().NotBeNull();
        _fiscalYearIds.Add(terzo!.Id);

        // Assert — primo: nessun predecessore
        primo.PreviousFiscalYearId.Should().BeNull(
            "il primo esercizio non ha predecessore");

        // Assert — secondo: punta al primo, data continua
        secondo.PreviousFiscalYearId.Should().Be(primo.Id,
            "il secondo esercizio deve referenziare il primo");
        secondo.StartDate.Date.Should().Be(primo.EndDate.Date.AddDays(1),
            "il secondo esercizio deve iniziare il giorno dopo la fine del primo");

        // Assert — terzo: punta al secondo, data continua
        terzo.PreviousFiscalYearId.Should().Be(secondo.Id,
            "il terzo esercizio deve referenziare il secondo");
        terzo.StartDate.Date.Should().Be(secondo.EndDate.Date.AddDays(1),
            "il terzo esercizio deve iniziare il giorno dopo la fine del secondo");

        // Verifica la catena completa via GET sulla lista del condominio
        var lista = await GetAsync<IList<FiscalYearListItemDto>>(
            $"/api/fiscal-years/by-condominium/{_condominiumId}");

        lista.Should().HaveCount(3, "devono esserci esattamente tre esercizi");

        var listaOrdinata = lista.OrderBy(x => x.StartDate).ToList();

        listaOrdinata[0].PreviousFiscalYearId.Should().BeNull(
            "il primo esercizio nella lista non ha predecessore");
        listaOrdinata[1].PreviousFiscalYearId.Should().Be(listaOrdinata[0].Id,
            "il secondo esercizio nella lista punta al primo");
        listaOrdinata[2].PreviousFiscalYearId.Should().Be(listaOrdinata[1].Id,
            "il terzo esercizio nella lista punta al secondo");

        // Verifica continuità delle date nella catena
        listaOrdinata[1].StartDate.Date.Should().Be(listaOrdinata[0].EndDate.Date.AddDays(1),
            "non devono esserci buchi tra primo e secondo esercizio");
        listaOrdinata[2].StartDate.Date.Should().Be(listaOrdinata[1].EndDate.Date.AddDays(1),
            "non devono esserci buchi tra secondo e terzo esercizio");
    }

    /// <summary>
    /// Un esercizio creato senza valorizzare <c>previousFiscalYearId</c> (anche quando
    /// esistono già esercizi per il condominio) deve avere <c>PreviousFiscalYearId = null</c>.
    /// Il collegamento è esplicito: il backend non inferisce automaticamente il predecessore.
    /// </summary>
    [Fact]
    public async Task EsercizioSenzaPrecedenteEsplicito_PreviousFiscalYearId_ENull()
    {
        // Arrange: crea un primo esercizio (non viene passato come predecessore)
        var (_, primo) = await PostAsync<FiscalYearReadDto>(
            "/api/fiscal-years", TestDataBuilder.FiscalYear(_condominiumId, year: 2023));
        primo.Should().NotBeNull();
        _fiscalYearIds.Add(primo!.Id);

        // Secondo esercizio: date non sovrapposte ma senza PreviousFiscalYearId
        var secondoDto = new FiscalYearCreateDto
        {
            CondominiumId        = _condominiumId,
            Code                 = $"2025-{TestDataBuilder.ShortId()}",
            Description          = "Esercizio 2025 (senza link esplicito)",
            StartDate            = new DateTime(2025, 1, 1),
            EndDate              = new DateTime(2025, 12, 31),
            PreviousFiscalYearId = null,   // esplicito: nessun predecessore
        };

        // Act
        var (createResponse, secondo) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", secondoDto);

        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "la creazione senza predecessore esplicito deve riuscire");
        secondo.Should().NotBeNull();
        _fiscalYearIds.Add(secondo!.Id);

        secondo.PreviousFiscalYearId.Should().BeNull(
            "l'omissione di previousFiscalYearId deve risultare in un campo null: " +
            "il backend non inferisce automaticamente il predecessore");
    }

    /// <summary>
    /// Un esercizio già referenziato come precedente da un altro esercizio non può essere
    /// riutilizzato: il secondo tentativo di usarlo come <c>previousFiscalYearId</c>
    /// deve restituire 400 Bad Request.
    ///
    /// Scenario:
    /// <code>
    ///   Primo  (2023) → nessun predecessore
    ///   Secondo(2024) → predecessore = Primo   (OK)
    ///   Terzo         → predecessore = Primo   (ERRORE: Primo già usato da Secondo)
    /// </code>
    /// La validazione del predecessore avviene prima della verifica di sovrapposizione,
    /// quindi l'errore "già collegato" viene restituito anche se le date si sovrappongono.
    /// </summary>
    [Fact]
    public async Task PrecedenteGiaUsato_Returns400()
    {
        // Arrange: crea il primo esercizio (2023)
        var (_, primo) = await PostAsync<FiscalYearReadDto>(
            "/api/fiscal-years", TestDataBuilder.FiscalYear(_condominiumId, year: 2023));
        primo.Should().NotBeNull();
        _fiscalYearIds.Add(primo!.Id);

        // Secondo: usa il primo come predecessore (OK)
        var (secondoResponse, secondo) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", new FiscalYearCreateDto
        {
            CondominiumId        = _condominiumId,
            Code                 = $"2024-{TestDataBuilder.ShortId()}",
            StartDate            = primo.EndDate.AddDays(1),
            EndDate              = primo.EndDate.AddYears(1),
            PreviousFiscalYearId = primo.Id,
        });
        secondoResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "il secondo esercizio deve essere creato come prerequisito del test");
        _fiscalYearIds.Add(secondo!.Id);

        // Terzo: tenta di riutilizzare il primo come predecessore → deve fallire
        var (terzoResponse, _) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", new FiscalYearCreateDto
        {
            CondominiumId        = _condominiumId,
            Code                 = $"DUP-{TestDataBuilder.ShortId()}",
            StartDate            = primo.EndDate.AddDays(1),   // corretta rispetto al predecessore dichiarato
            EndDate              = primo.EndDate.AddYears(1),
            PreviousFiscalYearId = primo.Id,                   // già usato da Secondo → non consentito
        });

        // Assert
        terzoResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "un esercizio già collegato come predecessore non può essere riutilizzato");

        var errorMessage = await ReadErrorAsync(terzoResponse);
        errorMessage.Should().Contain("già collegato",
            "il messaggio di errore deve indicare che il predecessore è già in uso");
    }

    /// <summary>
    /// Se la <c>StartDate</c> del nuovo esercizio non è esattamente il giorno successivo
    /// alla <c>EndDate</c> del predecessore, il backend deve rifiutare la richiesta con 400.
    ///
    /// Casi verificati:
    /// <list type="bullet">
    ///   <item>StartDate = EndDate del precedente (stesso giorno, non il successivo).</item>
    ///   <item>StartDate = EndDate + 2 giorni (un giorno di buco).</item>
    /// </list>
    /// </summary>
    [Fact]
    public async Task DataInizioNonContinua_ConPrecedente_Returns400()
    {
        // Arrange: crea il primo esercizio (EndDate = 2023-12-31)
        var (_, primo) = await PostAsync<FiscalYearReadDto>(
            "/api/fiscal-years", TestDataBuilder.FiscalYear(_condominiumId, year: 2023));
        primo.Should().NotBeNull();
        _fiscalYearIds.Add(primo!.Id);

        // Caso A: StartDate = stessa data di EndDate del precedente (non il giorno dopo)
        var (responseA, _) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", new FiscalYearCreateDto
        {
            CondominiumId        = _condominiumId,
            Code                 = $"A-{TestDataBuilder.ShortId()}",
            StartDate            = primo.EndDate,               // stesso giorno, non il successivo
            EndDate              = primo.EndDate.AddYears(1),
            PreviousFiscalYearId = primo.Id,
        });

        responseA.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "StartDate uguale a EndDate del precedente non è valida: " +
            "deve essere il giorno successivo (EndDate + 1)");

        var errorA = await ReadErrorAsync(responseA);
        errorA.Should().Contain("giorno successivo",
            "il messaggio deve indicare che la data di inizio deve essere il giorno successivo");

        // Caso B: StartDate = EndDate + 2 giorni (buco di un giorno)
        var (responseB, _) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", new FiscalYearCreateDto
        {
            CondominiumId        = _condominiumId,
            Code                 = $"B-{TestDataBuilder.ShortId()}",
            StartDate            = primo.EndDate.AddDays(2),    // due giorni dopo, non uno
            EndDate              = primo.EndDate.AddDays(2).AddYears(1).AddDays(-1),
            PreviousFiscalYearId = primo.Id,
        });

        responseB.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "StartDate con un giorno di buco rispetto al precedente non è valida");

        var errorB = await ReadErrorAsync(responseB);
        errorB.Should().Contain("giorno successivo",
            "il messaggio deve indicare che la data di inizio deve essere il giorno successivo");
    }
}
