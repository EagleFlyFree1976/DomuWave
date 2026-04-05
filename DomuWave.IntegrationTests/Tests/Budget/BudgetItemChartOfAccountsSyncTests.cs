using System.Net.Http.Json;
using DomuWave.IntegrationTests.Builders;
using DomuWave.IntegrationTests.Infrastructure;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Dto.ChartOfAccounts;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using DomuWave.Services.Models;
using FluentAssertions;
using System.Net;
using Xunit;
using ChartOfAccountsReadDto = DomuWave.Services.Dto.Budget.ChartOfAccountsReadDto;

namespace DomuWave.IntegrationTests.Tests.Budget;

/// <summary>
/// Test di integrazione per la sincronizzazione tra Piano dei Conti e Voci di Budget.
///
/// REGOLE DI BUSINESS VERIFICATE:
/// <list type="bullet">
///   <item>Alla creazione di un budget Preventivo, tutti i conti attivi del piano dei conti
///         vengono replicati automaticamente come voci di budget (snapshot di codice e nome).</item>
///   <item>La modifica del nome/codice di un conto propaga lo snapshot aggiornato alle voci
///         dei budget in stato <c>Draft</c>; i budget <c>Approved</c>/<c>Closed</c> rimangono invariati.</item>
///   <item>La cancellazione di un conto con voci in budget in bozza richiede un conto sostitutivo:
///         le voci vengono accorpate al sostitutivo (soft-delete della voce originale).</item>
///   <item>La cancellazione di un conto con voci solo in budget approvati/chiusi non richiede
///         sostitutivo: le voci storiche restano intatte (FK valida anche dopo soft-delete del conto).</item>
///   <item>Un conto creato dopo l'approvazione di un budget non compare nelle voci di quel budget:
///         il sync riguarda esclusivamente i budget in bozza.</item>
/// </list>
///
/// FLUSSO DI TEST (un unico <see cref="[Fact]"/> sequenziale):
/// <list type="number">
///   <item>Passo 1 (<see cref="InitializeAsync"/>): crea condominio, anno fiscale e piano dei conti minimo.</item>
///   <item>Passo 2: crea budget Preventivo → verifica replica voci da piano dei conti.</item>
///   <item>Passo 3: modifica nome conto A → verifica propagazione snapshot nelle voci in bozza.</item>
///   <item>Passo 4: cancella conto A (con B come sostitutivo) → verifica rimozione voce in bozza.</item>
///   <item>Passo 5: approva budget → modifica/cancella conti → verifica isolamento delle voci approvate.</item>
/// </list>
///
/// DIPENDENZE: <see cref="ApproveBudgetCommandConsumer"/> richiede almeno un conto per tipo
/// (Entrata, Uscita, Patrimoniale) prima di poter approvare un budget.
/// </summary>
[Collection("Integration")]
public class BudgetItemChartOfAccountsSyncTests(IntegrationTestFactory factory)
    : IntegrationTestBase(factory), IAsyncLifetime
{
    private int _condominiumId;
    private int _fiscalYearId;

    // Conto A: Uscita — conto principale per test di modifica e cancellazione
    private int    _accountAId;
    private string _accountACode = string.Empty;
    private string _accountAName = string.Empty;

    // Conto B: Uscita — sostitutivo del conto A nella cancellazione (passo 4)
    private int    _accountBId;
    private string _accountBCode = string.Empty;

    // Conti obbligatori per la validazione pre-approvazione (un conto per tipo)
    private int _accountEntrataId;
    private int _accountPatrimonialeId;

    /// <summary>Id dei budget creati nel test, per il cleanup in <see cref="DisposeAsync"/>.</summary>
    private readonly List<int> _budgetIds  = [];

    /// <summary>Id dei conti creati nel test, per il cleanup in <see cref="DisposeAsync"/>.</summary>
    private readonly List<int> _accountIds = [];

    // ── Lifecycle ──────────────────────────────────────────────────────────

    /// <summary>
    /// Passo 1 — Setup: crea il contesto condiviso da tutti i test di questa classe.
    ///
    /// Piano dei conti creato:
    /// <list type="bullet">
    ///   <item>Conto A (Uscita "Spese Generali"): modificato in passo 3, cancellato in passo 4.</item>
    ///   <item>Conto B (Uscita "Spese Manutenzione"): sostitutivo di A in passo 4, modificato in passo 5.</item>
    ///   <item>Conto Entrata e Conto Patrimoniale: richiesti da <c>ApproveBudgetCommandConsumer</c>
    ///         per la validazione pre-approvazione (passo 5).</item>
    /// </list>
    /// </summary>
    public override async Task InitializeAsync()
    {
        // ── Condominio ─────────────────────────────────────────────────────
        var (_, condo) = await PostAsync<CondominiumReadDto>(
            "/api/condominiums", TestDataBuilder.Condominium());
        _condominiumId = condo!.Id;

        // ── Anno fiscale ───────────────────────────────────────────────────
        var (_, fy) = await PostAsync<FiscalYearReadDto>(
            "/api/fiscal-years", TestDataBuilder.FiscalYear(_condominiumId));
        _fiscalYearId = fy!.Id;

        // ── Conto A: Uscita — soggetto ai test di modifica e cancellazione ─
        var (_, accA) = await PostAsync<ChartOfAccountsReadDto>("/api/chart-of-accounts",
            BuildAccount($"GEN-{TestDataBuilder.ShortId()}", "Spese Generali",
                         ChartOfAccountsType.Uscita));
        _accountAId   = accA!.Id;
        _accountACode = accA.Code;
        _accountAName = accA.Name;
        _accountIds.Add(_accountAId);

        // ── Conto B: Uscita — sostitutivo di A nella cancellazione ─────────
        var (_, accB) = await PostAsync<ChartOfAccountsReadDto>("/api/chart-of-accounts",
            BuildAccount($"MAN-{TestDataBuilder.ShortId()}", "Spese Manutenzione",
                         ChartOfAccountsType.Uscita));
        _accountBId   = accB!.Id;
        _accountBCode = accB.Code;
        _accountIds.Add(_accountBId);

        // ── Conto Entrata — obbligatorio per ApproveBudgetCommandConsumer ──
        var (_, accEnt) = await PostAsync<ChartOfAccountsReadDto>("/api/chart-of-accounts",
            BuildAccount($"ENT-{TestDataBuilder.ShortId()}", "Quote Condominiali",
                         ChartOfAccountsType.Entrata));
        _accountEntrataId = accEnt!.Id;
        _accountIds.Add(_accountEntrataId);

        // ── Conto Patrimoniale — obbligatorio per ApproveBudgetCommandConsumer
        var (_, accPat) = await PostAsync<ChartOfAccountsReadDto>("/api/chart-of-accounts",
            BuildAccount($"PAT-{TestDataBuilder.ShortId()}", "Fondo Cassa",
                         ChartOfAccountsType.Patrimoniale));
        _accountPatrimonialeId = accPat!.Id;
        _accountIds.Add(_accountPatrimonialeId);
    }

    /// <summary>
    /// Teardown best-effort in ordine inverso alle dipendenze FK:
    /// budget (riapertura + cancellazione) → conti → anno fiscale → condominio.
    ///
    /// Il budget creato nel test potrebbe essere in stato Approvato (non cancellabile direttamente):
    /// si tenta prima la riapertura (→ Draft) e poi la cancellazione.
    /// </summary>
    public override async Task DisposeAsync()
    {
        // Riapri (se approvato) e poi cancella ogni budget
        foreach (var id in _budgetIds)
        {
            try { await Client.PostAsJsonAsync($"/api/budgets/{id}/reopen", new { }); } catch { }
            try { await DeleteAsync($"/api/budgets/{id}"); }                          catch { }
        }

        // Cancella i conti (il conto A potrebbe essere già soft-deleted dal passo 4)
        foreach (var id in _accountIds)
        {
            try { await DeleteAsync($"/api/chart-of-accounts/{id}"); } catch { }
        }

        try { await DeleteAsync($"/api/fiscal-years/{_fiscalYearId}"); } catch { }
        try { await DeleteAsync($"/api/condominiums/{_condominiumId}"); } catch { }

        await base.DisposeAsync();
    }

    // ── Test ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Workflow sequenziale completo dei 5 passi descritti nel sommario della classe.
    /// Il test è strutturato come unico <c>[Fact]</c> perché ogni passo dipende dallo stato
    /// prodotto dal passo precedente (budget creato → modificato → approvato).
    /// </summary>
    [Fact]
    public async Task BudgetItem_SincronizzazioneConPianoConti_WorkflowCompleto()
    {
        // ══════════════════════════════════════════════════════════════════
        // Passo 2 — Crea budget Preventivo e verifica replica del piano dei conti
        // ══════════════════════════════════════════════════════════════════

        // CreateBudgetCommandConsumer: nessun Consuntivo precedente → crea una voce vuota
        // (Amount = 0) per ogni conto attivo del condominio.
        var (createResponse, budget) = await PostAsync<BudgetReadDto>(
            "/api/budgets", TestDataBuilder.Budget(_condominiumId, _fiscalYearId));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "la creazione del budget Preventivo deve riuscire con il piano dei conti configurato");
        budget.Should().NotBeNull();
        budget!.StatusId.Should().Be(BudgetStatus.Draft,
            "i budget devono sempre partire in stato Draft");
        _budgetIds.Add(budget.Id);

        // Recupera le voci di budget appena create
        var voci = await GetAsync<IList<BudgetItemReadDto>>(
            $"/api/budget-items/by-budget/{budget.Id}");

        voci.Should().NotBeEmpty(
            "il budget deve contenere una voce per ogni conto attivo del piano dei conti");

        // Verifica che tutti e 4 i conti siano stati replicati come voci di budget
        voci.Should().Contain(i => i.AccountId == _accountAId,
            "il conto A (Uscita) deve essere replicato come voce di budget");
        voci.Should().Contain(i => i.AccountId == _accountBId,
            "il conto B (Uscita) deve essere replicato come voce di budget");
        voci.Should().Contain(i => i.AccountId == _accountEntrataId,
            "il conto Entrata deve essere replicato come voce di budget");
        voci.Should().Contain(i => i.AccountId == _accountPatrimonialeId,
            "il conto Patrimoniale deve essere replicato come voce di budget");

        // Verifica che lo snapshot del conto A corrisponda ai dati del conto al momento della creazione
        var voceCreazioneA = voci.First(i => i.AccountId == _accountAId);
        voceCreazioneA.AccountCode.Should().Be(_accountACode,
            "lo snapshot AccountCode deve corrispondere al codice del conto alla creazione del budget");
        voceCreazioneA.AccountName.Should().Be(_accountAName,
            "lo snapshot AccountName deve corrispondere al nome del conto alla creazione del budget");
        voceCreazioneA.Amount.Should().Be(0m,
            "le voci create senza Consuntivo sorgente devono avere importo = 0");

        // ══════════════════════════════════════════════════════════════════
        // Passo 3 — Modifica nome del conto A, verifica propagazione snapshot
        // ══════════════════════════════════════════════════════════════════

        // Aggiorna il nome del conto A mantenendo invariati codice e tipo.
        // UpdateChartOfAccountsCommandConsumer.SyncDraftBudgetsAsync:
        //   - trova il budget in Draft del condominio
        //   - aggiorna AccountCode/AccountName sulla voce corrispondente
        var nuovoNomeA = $"Spese Generali MODIFICATO {TestDataBuilder.ShortId()}";

        var (updateAResponse, _) = await PutAsync<ChartOfAccountsReadDto>(
            $"/api/chart-of-accounts/{_accountAId}",
            BuildUpdateAccount(_accountACode, nuovoNomeA, ChartOfAccountsType.Uscita));

        updateAResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "l'aggiornamento del conto deve riuscire");

        // Rilegge le voci: lo snapshot del conto A deve riflettere il nuovo nome
        var vociDopoModifica = await GetAsync<IList<BudgetItemReadDto>>(
            $"/api/budget-items/by-budget/{budget.Id}");
        var voceADopoModifica = vociDopoModifica.FirstOrDefault(i => i.AccountId == _accountAId);

        voceADopoModifica.Should().NotBeNull(
            "la voce del conto A deve esistere ancora nel budget dopo la modifica del conto");
        voceADopoModifica!.AccountName.Should().Be(nuovoNomeA,
            "UpdateChartOfAccountsCommandConsumer deve aggiornare lo snapshot AccountName " +
            "nelle voci dei budget in stato Draft");
        voceADopoModifica.AccountCode.Should().Be(_accountACode,
            "il codice non è cambiato: lo snapshot AccountCode deve restare invariato");

        // ══════════════════════════════════════════════════════════════════
        // Passo 4 — Cancellazione conto A con sostitutivo B, verifica rimozione voce
        // ══════════════════════════════════════════════════════════════════

        // Il conto A ha voci in un budget in Draft → il sostitutivo è OBBLIGATORIO.
        // Comportamento del consumer con sostitutivo già presente nel budget:
        //   - la voce di A viene soft-deleted (IsDeleted = true)
        //   - l'importo di A (0) viene accorpato alla voce esistente di B
        // Comportamento sul conto A: soft-delete (IsDeleted = true sul conto).
        var deleteAResponse = await DeleteAsync(
            $"/api/chart-of-accounts/{_accountAId}?replacementAccountId={_accountBId}");

        deleteAResponse.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "la cancellazione con sostitutivo deve riuscire " +
            "anche se il conto A ha voci di budget in bozza");

        // Le voci soft-deleted non vengono restituite dal GET (filtro !IsDeleted su BudgetItem)
        var vociDopoDelete = await GetAsync<IList<BudgetItemReadDto>>(
            $"/api/budget-items/by-budget/{budget.Id}");

        vociDopoDelete.Should().NotContain(i => i.AccountId == _accountAId,
            "dopo la cancellazione del conto A la relativa voce di budget deve essere " +
            "rimossa (soft-deleted) e non più restituita dalla GET /api/budget-items/by-budget/{id}");

        // La voce del conto B (sostitutivo) deve essere ancora presente
        vociDopoDelete.Should().Contain(i => i.AccountId == _accountBId,
            "la voce del conto sostitutivo B deve esistere ancora nel budget");

        // ══════════════════════════════════════════════════════════════════
        // Passo 5 — Approvazione del budget e verifica isolamento delle voci approvate
        // ══════════════════════════════════════════════════════════════════

        // Memorizza lo snapshot del nome di B PRIMA dell'approvazione
        var voceBPrimaApprovazione = vociDopoDelete.First(i => i.AccountId == _accountBId);
        var snapshotNomeBPrimaApprovazione = voceBPrimaApprovazione.AccountName;

        // Approva il budget: il piano dei conti ha almeno un conto per tipo (Entrata/Uscita/Patrimoniale)
        var approveResponse = await ApproveBudgetAsync(budget.Id);
        approveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "l'approvazione deve riuscire: il piano dei conti è completo con tutti e tre i tipi");

        // ── Verifica 5a: modifica nome del conto B DOPO l'approvazione ────────────

        // UpdateChartOfAccountsCommandConsumer sincronizza solo i budget in bozza.
        // L'unico budget del condominio è ora Approvato → il sync non ha effetto.
        var nuovoNomeB = $"Spese Manutenzione POST-APPROVAZIONE {TestDataBuilder.ShortId()}";
        var (updateBResponse, _) = await PutAsync<ChartOfAccountsReadDto>(
            $"/api/chart-of-accounts/{_accountBId}",
            BuildUpdateAccount(_accountBCode, nuovoNomeB, ChartOfAccountsType.Uscita));

        updateBResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var vociApprovate = await GetAsync<IList<BudgetItemReadDto>>(
            $"/api/budget-items/by-budget/{budget.Id}");
        var voceBApprovata = vociApprovate.FirstOrDefault(i => i.AccountId == _accountBId);

        voceBApprovata.Should().NotBeNull();
        voceBApprovata!.AccountName.Should().Be(snapshotNomeBPrimaApprovazione,
            "i budget approvati NON devono essere aggiornati dal sync del piano dei conti: " +
            $"lo snapshot AccountName deve mantenere il valore '{snapshotNomeBPrimaApprovazione}' " +
            $"(quello al momento dell'approvazione), non il nuovo valore '{nuovoNomeB}'");

        // ── Verifica 5b: nuovo conto C creato DOPO l'approvazione ─────────────────

        // La creazione di un nuovo conto non triggera alcun sync su budget esistenti
        // (né in bozza né approvati). Il budget approvato non deve includere voci per il nuovo conto.
        var (_, contoC) = await PostAsync<ChartOfAccountsReadDto>("/api/chart-of-accounts",
            BuildAccount($"NEW-{TestDataBuilder.ShortId()}", "Conto Creato Post Approvazione",
                         ChartOfAccountsType.Uscita));
        contoC.Should().NotBeNull("la creazione del conto C deve riuscire");
        _accountIds.Add(contoC!.Id);

        var vociConContoC = await GetAsync<IList<BudgetItemReadDto>>(
            $"/api/budget-items/by-budget/{budget.Id}");
        vociConContoC.Should().NotContain(i => i.AccountId == contoC.Id,
            "un conto creato dopo l'approvazione del budget non deve comparire " +
            "nelle voci del budget approvato: il sync del piano dei conti agisce " +
            "esclusivamente sui budget in stato Draft");

        // ── Verifica 5c: cancellazione del conto B DOPO l'approvazione ────────────

        // Il conto B ha voci SOLO in un budget approvato (non in bozza).
        // DeleteChartOfAccountsCommandConsumer:
        //   - hasDraftUsages = false → sostitutivo NON obbligatorio
        //   - hasLockedUsages = true → le voci storiche restano intatte (FK valida via soft-delete)
        // Il conto B viene soft-deleted, ma la voce di budget rimane accessibile.
        var deleteBResponse = await DeleteAsync($"/api/chart-of-accounts/{_accountBId}");
        deleteBResponse.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "la cancellazione di un conto usato solo in budget approvati/chiusi deve riuscire " +
            "senza sostitutivo: le voci storiche rimangono valide perché la FK è sul BudgetItem " +
            "(non sul conto) e il soft-delete del conto non invalida la referenza");

        // Le voci del budget approvato devono essere ancora accessibili dopo la cancellazione del conto
        var vociDopoCancellazioneB = await GetAsync<IList<BudgetItemReadDto>>(
            $"/api/budget-items/by-budget/{budget.Id}");
        vociDopoCancellazioneB.Should().Contain(i => i.AccountId == _accountBId,
            "le voci di budget approvato devono restare intatte anche dopo il soft-delete " +
            "del conto: la FK BudgetItem.AccountId rimane valida, " +
            "e lo snapshot AccountCode/AccountName preserva i dati storici");
    }

    // ── Helpers privati ────────────────────────────────────────────────────

    /// <summary>
    /// Costruisce un <see cref="CreateChartOfAccountsDto"/> valido per il condominio corrente.
    /// Tutti i campi facoltativi vengono lasciati ai valori di default (nessuna categoria,
    /// metodo di ripartizione Standard, imputabilità al proprietario).
    /// </summary>
    private CreateChartOfAccountsDto BuildAccount(
        string code, string name, ChartOfAccountsType type) => new()
    {
        CondominiumId       = _condominiumId,
        Code                = code,
        Name                = name,
        Type                = type,
        IsActive            = true,
        AllocationMethod    = AllocationMethod.Standard,
        ChargeabilityTypeId = ChargeabilityType.Owner,
    };

    /// <summary>
    /// Costruisce un <see cref="UpdateChartOfAccountsDto"/> che mantiene i campi principali
    /// e aggiorna solo codice e nome. Usato nei passi 3 e 5 per testare la propagazione
    /// degli snapshot senza modificare tipo o metodo di ripartizione.
    /// </summary>
    private static UpdateChartOfAccountsDto BuildUpdateAccount(
        string code, string name, ChartOfAccountsType type) => new()
    {
        Code                = code,
        Name                = name,
        Type                = type,
        IsActive            = true,
        AllocationMethod    = AllocationMethod.Standard,
        ChargeabilityTypeId = ChargeabilityType.Owner,
    };

    /// <summary>
    /// Approva il budget tramite POST /api/budgets/{id}/approve con parametri standard:
    /// 4 rate trimestrali a partire da oggi.
    /// Restituisce il <see cref="HttpResponseMessage"/> grezzo per l'asserzione sul codice di stato.
    /// </summary>
    private Task<HttpResponseMessage> ApproveBudgetAsync(int budgetId)
        => Client.PostAsJsonAsync($"/api/budgets/{budgetId}/approve", new
        {
            NumberOfInstallments = 4,
            FirstDueDate         = DateTime.Today,
        });
}
