using System.Net;
using System.Net.Http.Json;
using DomuWave.IntegrationTests.Infrastructure;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Dto.ChartOfAccounts;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using DomuWave.Services.Dto.MillesimalTable;
using DomuWave.Services.Dto.RealEstateUnit;
using DomuWave.Services.Dto.UnitMillesimal;
using DomuWave.Services.Dto.UnitOwner;
using DomuWave.Services.Dto.UnitTenant;
using DomuWave.Services.Models;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace DomuWave.IntegrationTests.Tests.Seed;

/// <summary>
/// Seed di dati verosimili: Condominio "Il Giardino" — Via Giuseppe Garibaldi 12, Milano.
///
/// Struttura:
///   - 9 appartamenti (A01–A09) su 3 piani, 3 scale
///   - 8 box auto (B01–B08) — l'app. A09 (piano terra, portineria) non ha box
///   - 4 tabelle millesimali:
///       DEF  — Generale (proprietà)         1000.000
///       ASC  — Ascensore                    1000.000
///       GIAR — Giardino                     1000.000
///       CANC — Cancello elettrico            1000.000
///   - Piano dei conti con gerarchia Entrate / Uscite / Patrimoniale
///   - Occupanti: 9 proprietari (7 residenti, 2 non residenti); 1 inquilino (B06)
///   - Esercizio fiscale 2025 (01/01/2025 – 31/12/2025) → aperto
///   - Budget preventivo 2025 in bozza
///
/// Il test è rieseguibile: all'avvio elimina automaticamente il condominio con codice
/// "VGARI-12" se presente, poi ricrea tutto da zero.
/// </summary>
[Collection("Integration")]
public class SeedCondominioViaGaribaldiTest(
    IntegrationTestFactory factory,
    ITestOutputHelper output)
    : IntegrationTestBase(factory)
{
    // ── Costanti descrittive ───────────────────────────────────────────────
    private const string CondominioCode = "VGARI-13";
    private const string CondominioName = "Condominio Il Giardino";

    // ── Seed principale ────────────────────────────────────────────────────

    [Fact]
    public async Task Seed_CondominioViaGaribaldi_Complete()
    {
        output.WriteLine("=== SEED: Condominio Il Giardino, Via Garibaldi 12, Milano ===");

        // ── 0. Cleanup: elimina il condominio precedente se esiste ─────────
        var existingResp = await Client.GetAsync($"/api/condominiums/by-code/{CondominioCode}");
        if (existingResp.StatusCode == HttpStatusCode.OK)
        {
            var existing = await existingResp.Content.ReadFromJsonAsync<CondominiumReadDto>();
            if (existing != null)
            {
                // Elimina prima le tabelle millesimali esplicitamente per evitare che
                // il cascade soft-delete del condominio le lasci con IsDeleted=0
                // (bug di cache NHibernate nella stessa sessione).
                var tablesResp = await Client.GetAsync($"/api/millesimal-tables/by-condominium/{existing.Id}");
                if (tablesResp.IsSuccessStatusCode)
                {
                    var tables = await tablesResp.Content
                        .ReadFromJsonAsync<IList<MillesimalTableReadDto>>(JsonOptions);
                    foreach (var t in tables ?? [])
                        await Client.DeleteAsync($"/api/millesimal-tables/{t.Id}");
                }

                var delResp = await Client.DeleteAsync($"/api/condominiums/{existing.Id}");
                delResp.StatusCode.Should().Be(HttpStatusCode.NoContent,
                    $"cleanup condominio id={existing.Id}");
                output.WriteLine($"  Cleanup: condominio id={existing.Id} eliminato.");
            }
        }

        // ── 1. Condominio ──────────────────────────────────────────────────
        var condoDto = new CreateCondominiumDto
        {
            Code                 = CondominioCode,
            Name                 = CondominioName,
            TaxCode              = "93045670151",
            VatNumber            = "",
            Email                = "amministrazione@ilgiardino-mi.it",
            Phone                = "02 4893 2210",
            Pec                  = "ilgiardino.mi@pec.it",
            Notes                = "Condominio residenziale con giardino privato, ascensore e cancello elettrico.",
            InstallmentFrequency = "Monthly",
            InstallmentDueDay    = 5,
            NumberOfUnits        = 17,   // 9 app + 8 box
            NumberOfStaircases   = 3,
            NumberOfFloors       = 3,
            YearOfConstruction   = 1985,
            TotalMillesimal      = 1000m,
            HasElevator          = true,
            NumberOfElevators    = 1,
            HasCentralHeating    = false,
            HasConcierge         = false,
            CommonAreasSqm       = 420m,
            MandateStartDate     = new DateTime(2024, 1, 1),
            MandateEndDate       = new DateTime(2025, 12, 31),
            LastAssemblyDate     = new DateTime(2024, 11, 20),
            IsActive             = true,
            Address              = new CondominiumAddressDto
            {
                Street       = "Via Giuseppe Garibaldi",
                StreetNumber = "12",
                City         = "Milano",
                Province     = "MI",
                PostalCode   = "20121",
                Country      = "Italia",
            },
        };

        var (condoResp, condo) = await PostAsync<CondominiumReadDto>("/api/condominiums", condoDto);
        condoResp.StatusCode.Should().Be(HttpStatusCode.Created, await ReadErrorAsync(condoResp));
        var condId = condo!.Id;
        output.WriteLine($"  Condominio creato: id={condId}");

        // ── 2. Unità immobiliari ───────────────────────────────────────────
        // 9 appartamenti: 3 per scala (A, B, C), piani 1–3
        // 8 box: uno per ogni appartamento tranne A09 (portineria al piano terra)

        var apartments = new[]
        {
            // (scala, piano, numero, mq,    locali, abitanti, tipo,           saldoAp)
            ("A",  1, "A01", 78m,  3m, 2, "Appartamento", "Rossi Mario"),
            ("A",  2, "A02", 82m,  3m, 3, "Appartamento", "Bianchi Laura"),
            ("A",  3, "A03", 85m,  4m, 2, "Appartamento", "Ferri Giuseppe"),
            ("B",  1, "B04", 75m,  3m, 1, "Appartamento", "De Luca Anna"),
            ("B",  2, "B05", 80m,  3m, 2, "Appartamento", "Conti Sergio"),
            ("B",  3, "B06", 88m,  4m, 4, "Appartamento", "Mancini Lucia"),
            ("C",  1, "C07", 72m,  2m, 2, "Appartamento", "Ricci Carlo"),
            ("C",  2, "C08", 79m,  3m, 3, "Appartamento", "Esposito Sara"),
            ("C",  3, "C09", 90m,  4m, 2, "Appartamento", "Lombardi Pietro"),
        };

        var boxes = new[]
        {
            // (scala, interno, mq)
            ("A", "BOX-A01", 15m),
            ("A", "BOX-A02", 15m),
            ("A", "BOX-A03", 16m),
            ("B", "BOX-B04", 14m),
            ("B", "BOX-B05", 15m),
            ("B", "BOX-B06", 15m),
            ("C", "BOX-C07", 14m),
            ("C", "BOX-C08", 16m),
            // C09 non ha box (porteria/appartamento piccolo al piano terra)
        };

        var apIds = new Dictionary<string, int>();
        foreach (var (scala, piano, numero, mq, locali, abitanti, tipo, _) in apartments)
        {
            var dto = new CreateRealEstateUnitDto
            {
                CondominiumId   = condId,
                Staircase       = scala,
                Floor           = piano,
                InternalNumber  = numero,
                UnitType        = tipo,
                OccupancyStatus = "Owned",
                AreaSqm         = mq,
                Rooms           = locali,
                NumeroAbitanti  = abitanti,
                IsActive        = true,
            };
            var (r, unit) = await PostAsync<RealEstateUnitReadDto>("/api/real-estate-units", dto);
            r.StatusCode.Should().Be(HttpStatusCode.Created, $"appartamento {numero}");
            apIds[numero] = unit!.Id;
        }
        output.WriteLine($"  Appartamenti creati: {apIds.Count}");

        var boxIds = new Dictionary<string, int>();
        foreach (var (scala, numero, mq) in boxes)
        {
            var dto = new CreateRealEstateUnitDto
            {
                CondominiumId   = condId,
                Staircase       = scala,
                Floor           = -1,   // piano interrato
                InternalNumber  = numero,
                UnitType        = "Box",
                OccupancyStatus = "Owned",
                AreaSqm         = mq,
                Rooms           = 0,
                NumeroAbitanti  = 0,
                IsActive        = true,
            };
            var (r, unit) = await PostAsync<RealEstateUnitReadDto>("/api/real-estate-units", dto);
            r.StatusCode.Should().Be(HttpStatusCode.Created, $"box {numero}");
            boxIds[numero] = unit!.Id;
        }
        output.WriteLine($"  Box creati: {boxIds.Count}");

        // ── 3. Tabelle millesimali ────────────────────────────────────────
        // Millesimi verosimili calcolati proporzionalmente ai mq e alla posizione.
        // La somma di ciascuna tabella è esattamente 1000.000.

        // DEF — Generale (tutti gli appartamenti + box)
        // La tabella generale includerà sia appartamenti che box con pesi diversi.
        var defTable = await CreateMillesimalTableAsync(condId, "DEF", "Generale (proprietà)", 1000m);
        output.WriteLine($"  Tabella DEF creata: id={defTable.Id}");

        // ASC — Ascensore (solo appartamenti; i box non usano l'ascensore)
        var ascTable = await CreateMillesimalTableAsync(condId, "ASC", "Ascensore", 1000m);
        output.WriteLine($"  Tabella ASC creata: id={ascTable.Id}");

        // GIAR — Giardino (tutti gli appartamenti; i box non godono del giardino)
        var giarTable = await CreateMillesimalTableAsync(condId, "GIAR", "Giardino", 1000m);
        output.WriteLine($"  Tabella GIAR creata: id={giarTable.Id}");

        // CANC — Cancello elettrico (solo box e appartamenti con posto auto)
        var cancTable = await CreateMillesimalTableAsync(condId, "CANC", "Cancello elettrico", 1000m);
        output.WriteLine($"  Tabella CANC creata: id={cancTable.Id}");

        // ── Voci DEF (appartamenti + box, proporzionali ai mq totali) ─────
        // mq totali: 78+82+85+75+80+88+72+79+90 + 15+15+16+14+15+15+14+16 = 729+120 = 849
        // Il box vale circa 0.4 del peso dell'appartamento per mq.
        // Pesi effettivi: app × 1.0, box × 0.4. Somma ponderata = 729 + 120*0.4 = 777.
        // Millesimi app[i] = round(mq[i]/777 * 1000, 3), residuo sull'ultimo.
        var defApMillesimi = new[] { 100.386m, 105.534m, 109.395m, 96.524m, 102.960m, 113.257m, 92.664m, 101.672m, 115.830m };
        var defBoxMillesimi = new[] { 7.722m, 7.722m, 8.237m, 7.208m, 7.722m, 7.722m, 7.208m, 8.237m };
        // Somma attuale: 817.679 + rounding → aggiustiamo l'ultimo ap per raggiungere 1000
        // Somma box = 7.722*4 + 8.237*2 + 7.208*2 = 30.888 + 16.474 + 14.416 = 61.778
        // Somma ap = 1000 - 61.778 = 938.222 → adeguiamo C09 di conseguenza
        defApMillesimi[8] = 1000m - defApMillesimi[..8].Sum() - defBoxMillesimi.Sum();

        await SeedMillesimaliAsync(defTable.Id, apIds, defApMillesimi, boxIds, defBoxMillesimi);

        // ── Voci ASC (solo appartamenti) ──────────────────────────────────
        // Ascensore: pesi per piano (piano alto = uso maggiore). Piano 1: 0.5, 2: 1.0, 3: 1.5.
        // mq pesati: A01=78×0.5=39, A02=82×1=82, A03=85×1.5=127.5
        //            B04=75×0.5=37.5, B05=80×1=80, B06=88×1.5=132
        //            C07=72×0.5=36, C08=79×1=79, C09=90×1.5=135
        // Totale = 748; millesimi = mqPesato/748 * 1000
        var ascMillesimi = new[] { 52.139m, 109.626m, 170.455m, 50.134m, 106.952m, 176.471m, 48.128m, 105.615m, 180.481m };
        ascMillesimi[8] = 1000m - ascMillesimi[..8].Sum();
        await SeedMillesimaliAsync(ascTable.Id, apIds, ascMillesimi, null, null);

        // ── Voci GIAR (solo appartamenti) ─────────────────────────────────
        // Giardino: proporzionali ai mq appartamento (come ownership plain).
        // mqTotAp = 729
        var giarMillesimi = new[] { 106.995m, 112.483m, 116.598m, 102.881m, 109.739m, 120.713m, 98.767m, 108.368m, 123.456m };
        giarMillesimi[8] = 1000m - giarMillesimi[..8].Sum();
        await SeedMillesimaliAsync(giarTable.Id, apIds, giarMillesimi, null, null);

        // ── Voci CANC (solo box) ──────────────────────────────────────────
        // Cancello: diviso tra gli 8 box in proporzione ai mq.
        // mqTotBox = 120; BOX-A01=15, A02=15, A03=16, B04=14, B05=15, B06=15, C07=14, C08=16
        var cancMillesimi = new[] { 125.000m, 125.000m, 133.333m, 116.667m, 125.000m, 125.000m, 116.667m, 133.333m };
        cancMillesimi[7] = 1000m - cancMillesimi[..7].Sum();
        await SeedMillesimaliAsync(cancTable.Id, null, null, boxIds, cancMillesimi);

        output.WriteLine("  Voci millesimali inserite per tutte le tabelle.");

        // ── Abilita la tabella DEF ────────────────────────────────────────
        var enableResp = await Client.PatchAsync(
            $"/api/millesimal-tables/{defTable.Id}/enabled",
            JsonContent.Create(true));
        enableResp.StatusCode.Should().Be(HttpStatusCode.NoContent, await ReadErrorAsync(enableResp));
        output.WriteLine("  Tabella DEF abilitata.");

        // ── 4. Piano dei Conti ────────────────────────────────────────────
        var accountIds = new Dictionary<string, int>();

        async Task<int> CreateAccount(string code, string name, ChartOfAccountsType type,
            int? parentId = null, decimal budgetAmount = 0)
        {
            var dto = new CreateChartOfAccountsDto
            {
                CondominiumId          = condId,
                Code                   = code,
                Name                   = name,
                Type                   = type,
                ParentAccountId        = parentId,
                IsActive               = true,
                ChargeabilityTypeId    = ChargeabilityType.Owner,
                DefaultMillesimalTableId = defTable.Id,
            };
            var (r, acct) = await PostAsync<ChartOfAccountsReadDto>("/api/chart-of-accounts", dto);
            r.StatusCode.Should().Be(HttpStatusCode.Created, $"conto {code}");
            return acct!.Id;
        }

        // ── ENTRATE ────────────────────────────────────────────────────────
        var e0  = await CreateAccount("1",     "ENTRATE",                ChartOfAccountsType.Entrata);
        var e1  = await CreateAccount("1.1",   "Quote condominiali",     ChartOfAccountsType.Entrata, e0);
        /**/       await CreateAccount("1.1.1","Quote ordinarie",        ChartOfAccountsType.Entrata, e1);
        /**/       await CreateAccount("1.1.2","Quote straordinarie",    ChartOfAccountsType.Entrata, e1);
        var e2  = await CreateAccount("1.2",   "Interessi attivi",       ChartOfAccountsType.Entrata, e0);
        /**/       await CreateAccount("1.2.1","Interessi su c/c",       ChartOfAccountsType.Entrata, e2);
        var e3  = await CreateAccount("1.3",   "Rimborsi e recuperi",    ChartOfAccountsType.Entrata, e0);
        /**/       await CreateAccount("1.3.1","Rimborsi assicurativi",  ChartOfAccountsType.Entrata, e3);
        /**/       await CreateAccount("1.3.2","Recupero spese legali",  ChartOfAccountsType.Entrata, e3);

        // ── USCITE ────────────────────────────────────────────────────────
        var u0  = await CreateAccount("2",     "USCITE",                 ChartOfAccountsType.Uscita);

        var u1  = await CreateAccount("2.1",   "Pulizia e igiene",       ChartOfAccountsType.Uscita, u0);
        /**/       await CreateAccount("2.1.1","Servizio pulizie scale",  ChartOfAccountsType.Uscita, u1);
        /**/       await CreateAccount("2.1.2","Materiali pulizia",       ChartOfAccountsType.Uscita, u1);

        var u2  = await CreateAccount("2.2",   "Manutenzione ordinaria", ChartOfAccountsType.Uscita, u0);
        /**/       await CreateAccount("2.2.1","Manutenzione ascensore",  ChartOfAccountsType.Uscita, u2);
        /**/       await CreateAccount("2.2.2","Manutenzione cancello",   ChartOfAccountsType.Uscita, u2);
        /**/       await CreateAccount("2.2.3","Manutenzione giardino",   ChartOfAccountsType.Uscita, u2);
        /**/       await CreateAccount("2.2.4","Manutenzione impianti",   ChartOfAccountsType.Uscita, u2);
        /**/       await CreateAccount("2.2.5","Riparazioni varie",       ChartOfAccountsType.Uscita, u2);

        var u3  = await CreateAccount("2.3",   "Utenze comuni",          ChartOfAccountsType.Uscita, u0);
        /**/       await CreateAccount("2.3.1","Energia elettrica parti comuni", ChartOfAccountsType.Uscita, u3);
        /**/       await CreateAccount("2.3.2","Acqua condominiale",      ChartOfAccountsType.Uscita, u3);

        var u4  = await CreateAccount("2.4",   "Assicurazioni",          ChartOfAccountsType.Uscita, u0);
        /**/       await CreateAccount("2.4.1","Polizza globale fabbricati", ChartOfAccountsType.Uscita, u4);
        /**/       await CreateAccount("2.4.2","RC condominio",           ChartOfAccountsType.Uscita, u4);

        var u5  = await CreateAccount("2.5",   "Compensi e onorari",     ChartOfAccountsType.Uscita, u0);
        /**/       await CreateAccount("2.5.1","Onorario amministratore", ChartOfAccountsType.Uscita, u5);
        /**/       await CreateAccount("2.5.2","Consulenze tecniche",     ChartOfAccountsType.Uscita, u5);
        /**/       await CreateAccount("2.5.3","Spese legali",            ChartOfAccountsType.Uscita, u5);

        var u6  = await CreateAccount("2.6",   "Imposte e tasse",        ChartOfAccountsType.Uscita, u0);
        /**/       await CreateAccount("2.6.1","IMU parti comuni",        ChartOfAccountsType.Uscita, u6);
        /**/       await CreateAccount("2.6.2","Bollo e imposte varie",   ChartOfAccountsType.Uscita, u6);

        var u7  = await CreateAccount("2.7",   "Spese amministrative",   ChartOfAccountsType.Uscita, u0);
        /**/       await CreateAccount("2.7.1","Postali e cancelleria",   ChartOfAccountsType.Uscita, u7);
        /**/       await CreateAccount("2.7.2","Software gestionale",     ChartOfAccountsType.Uscita, u7);

        // ── PATRIMONIALE ──────────────────────────────────────────────────
        var p0  = await CreateAccount("3",     "PATRIMONIALE",           ChartOfAccountsType.Patrimoniale);
        var p1  = await CreateAccount("3.1",   "Fondi di riserva",       ChartOfAccountsType.Patrimoniale, p0);
        /**/       await CreateAccount("3.1.1","Fondo ordinario",        ChartOfAccountsType.Patrimoniale, p1);
        /**/       await CreateAccount("3.1.2","Fondo straordinario",    ChartOfAccountsType.Patrimoniale, p1);
        var p2  = await CreateAccount("3.2",   "Conti correnti",         ChartOfAccountsType.Patrimoniale, p0);
        /**/       await CreateAccount("3.2.1","C/C bancario ordinario", ChartOfAccountsType.Patrimoniale, p2);
        var p3  = await CreateAccount("3.3",   "Crediti vs condòmini",   ChartOfAccountsType.Patrimoniale, p0);
        /**/       await CreateAccount("3.3.1","Morosità in corso",      ChartOfAccountsType.Patrimoniale, p3);

        output.WriteLine("  Piano dei conti creato.");

        // ── 5. Esercizio fiscale 2025 ─────────────────────────────────────
        var fyDto = new FiscalYearCreateDto
        {
            CondominiumId = condId,
            Code          = "2025",
            Description   = "Esercizio 2025",
            StartDate     = new DateTime(2025, 1, 1),
            EndDate       = new DateTime(2025, 12, 31),
        };
        var (fyResp, fy) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", fyDto);
        fyResp.StatusCode.Should().Be(HttpStatusCode.Created, await ReadErrorAsync(fyResp));
        var fyId = fy!.Id;
        output.WriteLine($"  Esercizio 2025 creato: id={fyId}");

        // Apertura esercizio (Draft → Open)
        var openResp = await Client.PostAsJsonAsync($"/api/fiscal-years/{fyId}/open", new { });
        openResp.StatusCode.Should().Be(HttpStatusCode.NoContent, await ReadErrorAsync(openResp));
        output.WriteLine("  Esercizio 2025 aperto.");

        // ── 6. Occupanti ──────────────────────────────────────────────────
        // 8 appartamenti occupati solo da proprietario (di cui 2 non residenti).
        // 1 appartamento (B06 – Mancini Lucia) in affitto: proprietario non residente + inquilino.
        // I box non hanno occupanti (uso diretto del proprietario).

        // (apKey, firstName, lastName, taxCode, email, ownerType, quota, isResident)
        var owners = new[]
        {
            ("A01", "Mario",    "Rossi",     "RSSMRA70A01F205X", "mario.rossi@email.it",       "Privato", 1.000m, true),
            ("A02", "Laura",    "Bianchi",   "BNCLRA75B41F205Y", "laura.bianchi@email.it",      "Privato", 1.000m, true),
            ("A03", "Giuseppe", "Ferri",     "FRRGPP68C15F205Z", "giuseppe.ferri@email.it",     "Privato", 1.000m, true),
            ("B04", "Anna",     "De Luca",   "DLCNNA80D45F205W", "anna.deluca@email.it",        "Privato", 1.000m, true),
            ("B05", "Sergio",   "Conti",     "CNTSGR65E01F205V", "sergio.conti@email.it",       "Privato", 1.000m, true),
            // B06: proprietario non residente (ha dato in affitto)
            ("B06", "Lucia",    "Mancini",   "MNCLCU72F55F205U", "lucia.mancini@email.it",      "Privato", 1.000m, false),
            ("C07", "Carlo",    "Ricci",     "RCCCRL58G01F205T", "carlo.ricci@email.it",        "Privato", 1.000m, true),
            ("C08", "Sara",     "Esposito",  "SPSRSA90H45F205S", "sara.esposito@email.it",      "Privato", 1.000m, true),
            // C09: proprietario non residente (figlio vive altrove, immobile non occupato)
            ("C09", "Pietro",   "Lombardi",  "LMBPTR55I01F205R", "pietro.lombardi@email.it",    "Privato", 1.000m, false),
        };

        foreach (var (apKey, firstName, lastName, taxCode, email, ownerType, quota, isResident) in owners)
        {
            var dto = new CreateUnitOwnerDto
            {
                UnitId          = apIds[apKey],
                UserId          = 0,   // nessun account portale associato
                FirstName       = firstName,
                LastName        = lastName,
                Email           = email,
                OwnerType       = ownerType,
                OwnershipQuota  = quota,
                StartDate       = new DateTime(2020, 1, 1),
                EndDate         = null,
                IsResident      = isResident,
                IsActive        = true,
                IsAccessEnabled = false,
            };
            var (r, _) = await PostAsync<UnitOwnerReadDto>($"/api/unit-owners", dto);
            r.StatusCode.Should().Be(HttpStatusCode.Created, $"proprietario {apKey}");
        }
        output.WriteLine("  Proprietari creati (9).");

        // Inquilino di B06 (Mancini Lucia ha dato in affitto a Famiglia Ferretti)
        var tenantDto = new CreateUnitTenantDto
        {
            UnitId         = apIds["B06"],
            UserId         = 0,
            FirstName      = "Roberto",
            LastName       = "Ferretti",
            TaxCode        = "FRTRBT88L10F205P",
            Email          = "roberto.ferretti@email.it",
            Phone          = "339 4512 8877",
            LeaseStartDate = new DateTime(2023, 3, 1),
            LeaseEndDate   = new DateTime(2025, 2, 28),
            ExpensePayer   = "Owner",   // spese condominiali a carico del proprietario
            IsActive       = true,
            Notes          = "Contratto 4+4, locazione abitativa.",
        };
        var (tResp, _) = await PostAsync<UnitTenantReadDto>("/api/unit-tenants", tenantDto);
        tResp.StatusCode.Should().Be(HttpStatusCode.Created, "inquilino B06");
        output.WriteLine("  Inquilino B06 (Ferretti Roberto) creato.");

        // ── 8. Budget preventivo 2025 (Bozza) ────────────────────────────
        var budgetDto = new CreateBudgetDto
        {
            CondominiumId = condId,
            FiscalYearId  = fyId,
            Type          = BudgetType.Preventivo,
            Notes         = "Preventivo 2025 – condominio Il Giardino",
        };
        var (budgResp, budget) = await PostAsync<BudgetReadDto>("/api/budgets", budgetDto);
        budgResp.StatusCode.Should().Be(HttpStatusCode.Created, await ReadErrorAsync(budgResp));
        output.WriteLine($"  Budget preventivo 2025 creato: id={budget!.Id}");

        output.WriteLine("");
        output.WriteLine($"  ✓ Seed completato.");
        output.WriteLine($"    CondominiumId  = {condId}");
        output.WriteLine($"    FiscalYearId   = {fyId}");
        output.WriteLine($"    BudgetId       = {budget.Id}");
        output.WriteLine($"    Tabella DEF    = {defTable.Id}");
        output.WriteLine($"    Tabella ASC    = {ascTable.Id}");
        output.WriteLine($"    Tabella GIAR   = {giarTable.Id}");
        output.WriteLine($"    Tabella CANC   = {cancTable.Id}");
        output.WriteLine($"    Proprietari    = 9 (7 residenti, 2 non residenti)");
        output.WriteLine($"    Inquilini      = 1 (B06 — Ferretti Roberto)");
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private async Task<MillesimalTableReadDto> CreateMillesimalTableAsync(
        int condId, string code, string name, decimal total)
    {
        // Il condominio potrebbe aver già creato automaticamente la tabella DEF:
        // se esiste già una tabella con lo stesso codice, la usiamo direttamente.
        var existingResp = await Client.GetAsync(
            $"/api/millesimal-tables/by-condominium/{condId}/code/{code}");
        if (existingResp.StatusCode == HttpStatusCode.OK)
        {
            var existing = await existingResp.Content
                .ReadFromJsonAsync<MillesimalTableReadDto>(JsonOptions);
            if (existing != null)
                return existing;
        }

        var dto = new CreateMillesimalTableDto
        {
            CondominiumId   = condId,
            Code            = code,
            Name            = name,
            TotalMillesimal = total,
        };
        var (r, table) = await PostAsync<MillesimalTableReadDto>("/api/millesimal-tables", dto);
        r.StatusCode.Should().Be(HttpStatusCode.Created, await ReadErrorAsync(r));
        return table!;
    }

    /// <summary>
    /// Inserisce le voci millesimali per gli appartamenti e/o i box in una tabella.
    /// Gli array <paramref name="apMillesimi"/> e <paramref name="boxMillesimi"/> devono
    /// avere lo stesso ordine degli array globali <c>apartments</c> e <c>boxes</c>.
    /// Passare null per saltare un gruppo.
    /// </summary>
    private async Task SeedMillesimaliAsync(
        int tableId,
        Dictionary<string, int>? apIds,
        decimal[]? apMillesimi,
        Dictionary<string, int>? boxIds,
        decimal[]? boxMillesimi)
    {
        if (apIds != null && apMillesimi != null)
        {
            var apKeys = apIds.Keys.OrderBy(k => k).ToArray();
            for (int i = 0; i < apKeys.Length; i++)
            {
                var dto = new CreateUnitMillesimalDto
                {
                    MillesimalTableId = tableId,
                    UnitId            = apIds[apKeys[i]],
                    Millesimal        = apMillesimi[i],
                };
                var (r, _) = await PostAsync<UnitMillesimalReadDto>("/api/unit-millesimals", dto);
                r.StatusCode.Should().Be(HttpStatusCode.Created, $"millesimale app {apKeys[i]} tab {tableId}");
            }
        }

        if (boxIds != null && boxMillesimi != null)
        {
            var boxKeys = boxIds.Keys.OrderBy(k => k).ToArray();
            for (int i = 0; i < boxKeys.Length; i++)
            {
                var dto = new CreateUnitMillesimalDto
                {
                    MillesimalTableId = tableId,
                    UnitId            = boxIds[boxKeys[i]],
                    Millesimal        = boxMillesimi[i],
                };
                var (r, _) = await PostAsync<UnitMillesimalReadDto>("/api/unit-millesimals", dto);
                r.StatusCode.Should().Be(HttpStatusCode.Created, $"millesimale box {boxKeys[i]} tab {tableId}");
            }
        }
    }
}
