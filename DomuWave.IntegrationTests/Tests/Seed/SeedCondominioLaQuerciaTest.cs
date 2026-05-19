using System.Net;
using System.Net.Http.Json;
using DomuWave.IntegrationTests.Infrastructure;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Dto.ChartOfAccounts;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using DomuWave.Services.Dto.Expense;
using DomuWave.Services.Dto.MillesimalTable;
using DomuWave.Services.Dto.RealEstateUnit;
using DomuWave.Services.Dto.Supplier;
using DomuWave.Services.Dto.UnitMillesimal;
using DomuWave.Services.Dto.UnitOpeningBalance;
using DomuWave.Services.Dto.UnitOwner;
using DomuWave.Services.Models;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace DomuWave.IntegrationTests.Tests.Seed;

/// <summary>
/// Seed di dati verosimili: Condominio "La Quercia" — Via delle Querce 14, Milano.
///
/// Struttura:
///   - 9 appartamenti (A01–A03, B01–B03, C01–C03) su 3 scale, 3 piani
///   - 3 box auto (BOX-A1, BOX-B1, BOX-C1) — uno per scala
///   - 1 tabella millesimale generale (TMG)
///   - Piano dei conti con gerarchia Entrate / Uscite / Patrimoniale
///   - 12 proprietari (uno per unità)
///   - Esercizio 2024 chiuso con preventivo + consuntivo approvati
///   - 6 spese reali, 4 rate trimestrali tutte pagate tranne:
///       - Galli Sofia (B01): Q4 non pagata — morosità 187.12 €
///       - Greco Paolo (C03): Q3+Q4 non pagate — morosità 748.72 €
///
/// Il test è rieseguibile: elimina il condominio con codice "LQ-001" se già presente.
/// </summary>
[Collection("Integration")]
public class SeedCondominioLaQuerciaTest(
    IntegrationTestFactory factory,
    ITestOutputHelper output)
    : IntegrationTestBase(factory)
{
    private const string CondominioCode = "LQ-003";
    private const string CondominioName = "Condominio Il Pioppo";

    [Fact]
    public async Task Seed_CondominioLaQuercia_Complete()
    {
        output.WriteLine("=== SEED: Condominio La Quercia, Via delle Querce 14, Milano ===");

        // ── 0. Cleanup: hard-delete di tutti i record con questo codice ──────
        await PurgeCondominiumByCodeAsync(CondominioCode);
        output.WriteLine($"  Cleanup: purge '{CondominioCode}' completato.");

        // ── 1. Condominio ─────────────────────────────────────────────────────
        var condoDto = new CreateCondominiumDto
        {
            Code                 = CondominioCode,
            Name                 = CondominioName,
            TaxCode              = "91054320153",
            VatNumber            = "",
            Email                = "amministrazione@laquercia-mi.it",
            Phone                = "02 5512 3344",
            Pec                  = "laquercia.mi@pec.it",
            Notes                = "Condominio residenziale con 3 scale, esercizio 2024 chiuso.",
            InstallmentFrequency = "Quarterly",
            InstallmentDueDay    = 15,
            NumberOfUnits        = 12,
            NumberOfStaircases   = 3,
            NumberOfFloors       = 3,
            YearOfConstruction   = 1978,
            TotalMillesimal      = 1000m,
            HasElevator          = false,
            HasCentralHeating    = false,
            HasConcierge         = false,
            CommonAreasSqm       = 210m,
            MandateStartDate     = new DateTime(2023, 1, 1),
            MandateEndDate       = new DateTime(2025, 12, 31),
            LastAssemblyDate     = new DateTime(2025, 1, 20),
            IsActive             = true,
            Address              = new CondominiumAddressDto
            {
                Street       = "Via delle Querce",
                StreetNumber = "14",
                City         = "Milano",
                Province     = "MI",
                PostalCode   = "20141",
                Country      = "Italia",
            },
        };

        var (condoResp, condo) = await PostAsync<CondominiumReadDto>("/api/condominiums", condoDto);
        condoResp.StatusCode.Should().Be(HttpStatusCode.Created, await ReadErrorAsync(condoResp));
        var condId = condo!.Id;
        output.WriteLine($"  Condominio creato: id={condId}");

        // Recupera tabella DEF auto-creata col condominio
        var tablesInitResp = await Client.GetAsync($"/api/millesimal-tables/by-condominium/{condId}");
        tablesInitResp.IsSuccessStatusCode.Should().BeTrue("get tabelle millesimali iniziali");
        var tablesInit = await tablesInitResp.Content.ReadFromJsonAsync<IList<MillesimalTableReadDto>>(JsonOptions);
        var defTable   = tablesInit?.FirstOrDefault(t => t.Code == "DEF");
        output.WriteLine($"  Tabella DEF trovata: id={defTable?.Id}");

        // ── 2. Unità immobiliari ──────────────────────────────────────────────
        // 9 appartamenti su 3 scale × 3 piani + 3 box
        var apartments = new[]
        {
            // (scala, piano, numero, mq,    locali, abitanti, displayName)
            ("A", 1, "A01",  85m, 3m, 2, "Ferrari"),
            ("A", 2, "A02", 110m, 4m, 3, "Marchetti"),
            ("A", 3, "A03",  90m, 3m, 2, "Colombo"),
            ("B", 1, "B01",  65m, 2m, 1, "Galli"),
            ("B", 2, "B02", 120m, 4m, 3, "Fontana"),
            ("B", 3, "B03",  95m, 3m, 2, "Rizzo"),
            ("C", 1, "C01",  80m, 3m, 2, "Barbieri"),
            ("C", 2, "C02",  60m, 2m, 1, "Conti"),
            ("C", 3, "C03", 130m, 4m, 3, "Greco"),
        };

        var boxes = new[]
        {
            // (scala, numero, mq)
            ("A", "BOX-A1", 14m),
            ("B", "BOX-B1", 14m),
            ("C", "BOX-C1", 14m),
        };

        var apIds = new Dictionary<string, int>();
        foreach (var (scala, piano, numero, mq, locali, abitanti, _) in apartments)
        {
            var dto = new CreateRealEstateUnitDto
            {
                CondominiumId   = condId,
                Floor           = piano,
                InternalNumber  = numero,
                UnitType        = "Appartamento",
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
                Floor           = -1,
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

        // ── 3. Tabella millesimale ────────────────────────────────────────────
        // TMG — Generale (tutti appartamenti + box)
        // mq appartamenti: 85+110+90+65+120+95+80+60+130 = 835
        // mq box: 14×3 = 42 (peso 0.4 → 16.8 equivalenti)
        // Millesimi proporzionali ai mq (box a peso 0.4).
        // Somma ponderata = 835 + 16.8 = 851.8
        var tmgTable = await CreateMillesimalTableAsync(condId, "TMG", "Tabella Millesimale Generale", 1000m);
        output.WriteLine($"  Tabella TMG creata: id={tmgTable.Id}");

        // Appartamenti: millesimi proporzionali ai mq (tot. proporzionale 851.8 m²)
        // I valori sono precalcolati; C03 è aggiustato per portare il totale esattamente a 1000.
        //   Somma A01..C02 + BOX-A1..BOX-C1 (stimati) = 840.8453 + 13.1452 = 853.9905
        //   Somma A01..C02 = 827.7001
        //   BOX fissi = 6.5726 + 6.5726 + 6.5241 = 19.6693
        //   Residuo C03 = 1000 - 827.7001 - 19.6693 = 152.6306
        var tmgApMillesimi = new[]
        {
             99.7888m,  // A01  85 mq
            129.1289m,  // A02 110 mq
            105.6680m,  // A03  90 mq
             76.3088m,  // B01  65 mq
            140.8924m,  // B02 120 mq
            111.5483m,  // B03  95 mq
             93.9228m,  // C01  80 mq
             70.4421m,  // C02  60 mq
            152.6306m,  // C03 130 mq — aggiustato per chiudere a 1000
        };
        var tmgBoxMillesimi = new[]
        {
             6.5726m,   // BOX-A1 14 mq
             6.5726m,   // BOX-B1 14 mq
             6.5241m,   // BOX-C1 14 mq
        };
        // Verifica che il totale sia esattamente 1000
        System.Diagnostics.Debug.Assert(
            tmgApMillesimi.Sum() + tmgBoxMillesimi.Sum() == 1000m,
            $"Totale millesimi = {tmgApMillesimi.Sum() + tmgBoxMillesimi.Sum()}");

        await SeedMillesimaliAsync(tmgTable.Id, apIds, tmgApMillesimi, boxIds, tmgBoxMillesimi);

        // Popola anche la tabella DEF (stessi millesimi TMG — richiesta per approvazione budget)
        if (defTable != null)
        {
            await SeedMillesimaliAsync(defTable.Id, apIds, tmgApMillesimi, boxIds, tmgBoxMillesimi);
            output.WriteLine($"  Tabella DEF popolata ({apIds.Count + boxIds.Count} voci).");
        }

        // Abilita tabella TMG
        var enableResp = await Client.PatchAsync(
            $"/api/millesimal-tables/{tmgTable.Id}/enabled",
            JsonContent.Create(true));
        enableResp.StatusCode.Should().Be(HttpStatusCode.NoContent, await ReadErrorAsync(enableResp));
        output.WriteLine("  Tabella TMG abilitata.");

        // ── 4. Piano dei conti ────────────────────────────────────────────────
        async Task<int> CreateAccount(string code, string name, ChartOfAccountsType type,
            int? parentId = null)
        {
            var dto = new CreateChartOfAccountsDto
            {
                CondominiumId            = condId,
                Code                     = code,
                Name                     = name,
                Type                     = type,
                ParentAccountId          = parentId,
                IsActive                 = true,
                ChargeabilityTypeId      = ChargeabilityType.Owner,
                DefaultMillesimalTableId = tmgTable.Id,
            };
            var (r, acct) = await PostAsync<ChartOfAccountsReadDto>("/api/chart-of-accounts", dto);
            r.StatusCode.Should().Be(HttpStatusCode.Created, $"conto {code}");
            return acct!.Id;
        }

        // ENTRATE
        var e0 = await CreateAccount("1",     "ENTRATE",                  ChartOfAccountsType.Entrata);
        var e1 = await CreateAccount("1.1",   "Quote condominiali",       ChartOfAccountsType.Entrata, e0);
              await CreateAccount("1.1.1", "Quote ordinarie",             ChartOfAccountsType.Entrata, e1);
              await CreateAccount("1.1.2", "Quote straordinarie",         ChartOfAccountsType.Entrata, e1);
        var e2 = await CreateAccount("1.2",   "Interessi e recuperi",     ChartOfAccountsType.Entrata, e0);
              await CreateAccount("1.2.1", "Interessi attivi",            ChartOfAccountsType.Entrata, e2);
              await CreateAccount("1.2.2", "Rimborsi assicurativi",       ChartOfAccountsType.Entrata, e2);

        // USCITE
        var u0 = await CreateAccount("2",     "USCITE",                   ChartOfAccountsType.Uscita);
        var u1 = await CreateAccount("2.1",   "Pulizia e igiene",         ChartOfAccountsType.Uscita, u0);
        var accPulizie = await CreateAccount("2.1.1", "Servizio pulizie", ChartOfAccountsType.Uscita, u1);
              await CreateAccount("2.1.2", "Materiali pulizia",           ChartOfAccountsType.Uscita, u1);
        var u2 = await CreateAccount("2.2",   "Manutenzione ordinaria",   ChartOfAccountsType.Uscita, u0);
        var accManutenzione = await CreateAccount("2.2.1", "Manutenzione parti comuni", ChartOfAccountsType.Uscita, u2);
              await CreateAccount("2.2.2", "Riparazioni impianti",        ChartOfAccountsType.Uscita, u2);
              await CreateAccount("2.2.3", "Manutenzione cancello",       ChartOfAccountsType.Uscita, u2);
        var u3 = await CreateAccount("2.3",   "Utenze comuni",            ChartOfAccountsType.Uscita, u0);
        var accUtenze = await CreateAccount("2.3.1", "Energia elettrica scale", ChartOfAccountsType.Uscita, u3);
              await CreateAccount("2.3.2", "Acqua condominiale",          ChartOfAccountsType.Uscita, u3);
        var u4 = await CreateAccount("2.4",   "Assicurazioni",            ChartOfAccountsType.Uscita, u0);
        var accAssicurazione = await CreateAccount("2.4.1", "Polizza globale fabbricati", ChartOfAccountsType.Uscita, u4);
        var u5 = await CreateAccount("2.5",   "Compensi e onorari",       ChartOfAccountsType.Uscita, u0);
        var accAmministrazione = await CreateAccount("2.5.1", "Onorario amministratore", ChartOfAccountsType.Uscita, u5);
              await CreateAccount("2.5.2", "Consulenze tecniche",         ChartOfAccountsType.Uscita, u5);
        var u6 = await CreateAccount("2.6",   "Spese amministrative",     ChartOfAccountsType.Uscita, u0);
              await CreateAccount("2.6.1", "Postali e cancelleria",       ChartOfAccountsType.Uscita, u6);
        var u7 = await CreateAccount("2.7",   "Fondo riserva",            ChartOfAccountsType.Uscita, u0);
        var accFondoRiserva = await CreateAccount("2.7.1", "Fondo riserva ordinario", ChartOfAccountsType.Uscita, u7);

        // PATRIMONIALE
        var p0 = await CreateAccount("3",     "PATRIMONIALE",             ChartOfAccountsType.Patrimoniale);
        var p1 = await CreateAccount("3.1",   "Fondi",                    ChartOfAccountsType.Patrimoniale, p0);
              await CreateAccount("3.1.1", "Fondo ordinario",             ChartOfAccountsType.Patrimoniale, p1);
        var p2 = await CreateAccount("3.2",   "Conti correnti",           ChartOfAccountsType.Patrimoniale, p0);
              await CreateAccount("3.2.1", "C/C bancario",                ChartOfAccountsType.Patrimoniale, p2);
        var p3 = await CreateAccount("3.3",   "Crediti vs condòmini",     ChartOfAccountsType.Patrimoniale, p0);
              await CreateAccount("3.3.1", "Morosità in corso",           ChartOfAccountsType.Patrimoniale, p3);

        output.WriteLine("  Piano dei conti creato.");

        // ── 5. Proprietari ────────────────────────────────────────────────────
        // (apKey, firstName, lastName, email, isResident)
        var owners = new[]
        {
            ("A01", "Marco",    "Ferrari",   "marco.ferrari@email.it",    true),
            ("A02", "Laura",    "Marchetti", "laura.marchetti@email.it",  true),
            ("A03", "Giuseppe", "Colombo",   "giuseppe.colombo@email.it", true),
            ("B01", "Sofia",    "Galli",     "sofia.galli@email.it",      true),
            ("B02", "Roberto",  "Fontana",   "roberto.fontana@email.it",  true),
            ("B03", "Anna",     "Rizzo",     "anna.rizzo@email.it",       true),
            ("C01", "Luca",     "Barbieri",  "luca.barbieri@email.it",    true),
            ("C02", "Elena",    "Conti",     "elena.conti@email.it",      true),
            ("C03", "Paolo",    "Greco",     "paolo.greco@email.it",      true),
        };

        foreach (var (apKey, firstName, lastName, email, isResident) in owners)
        {
            var dto = new CreateUnitOwnerDto
            {
                UnitId          = apIds[apKey],
                UserId          = 0,
                FirstName       = firstName,
                LastName        = lastName,
                Email           = email,
                OwnerType       = "Privato",
                OwnershipQuota  = 1.000m,
                StartDate       = new DateTime(2020, 1, 1),
                EndDate         = null,
                IsResident      = isResident,
                IsActive        = true,
                IsAccessEnabled = false,
            };
            var (r, _) = await PostAsync<UnitOwnerReadDto>("/api/unit-owners", dto);
            r.StatusCode.Should().Be(HttpStatusCode.Created, $"proprietario {apKey} - {lastName}");
        }

        // Proprietari dei box (stesse persone degli appartamenti corrispondenti)
        var boxOwners = new[]
        {
            ("BOX-A1", "Marco",   "Ferrari",  "marco.ferrari@email.it"),
            ("BOX-B1", "Roberto", "Fontana",  "roberto.fontana@email.it"),
            ("BOX-C1", "Paolo",   "Greco",    "paolo.greco@email.it"),
        };
        foreach (var (boxKey, firstName, lastName, email) in boxOwners)
        {
            var dto = new CreateUnitOwnerDto
            {
                UnitId          = boxIds[boxKey],
                UserId          = 0,
                FirstName       = firstName,
                LastName        = lastName,
                Email           = email,
                OwnerType       = "Privato",
                OwnershipQuota  = 1.000m,
                StartDate       = new DateTime(2020, 1, 1),
                EndDate         = null,
                IsResident      = false,
                IsActive        = true,
                IsAccessEnabled = false,
            };
            var (r, _) = await PostAsync<UnitOwnerReadDto>("/api/unit-owners", dto);
            r.StatusCode.Should().Be(HttpStatusCode.Created, $"proprietario {boxKey}");
        }
        output.WriteLine("  Proprietari creati (12 totale: 9 apt + 3 box).");

        // ── 6. Esercizio fiscale 2024 ─────────────────────────────────────────
        var fyDto = new FiscalYearCreateDto
        {
            CondominiumId = condId,
            Code          = "2024",
            Description   = "Esercizio 2024",
            StartDate     = new DateTime(2024, 1, 1),
            EndDate       = new DateTime(2024, 12, 31),
        };
        var (fyResp, fy) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", fyDto);
        fyResp.StatusCode.Should().Be(HttpStatusCode.Created, await ReadErrorAsync(fyResp));
        var fyId = fy!.Id;
        output.WriteLine($"  Esercizio 2024 creato: id={fyId}");

        // Saldi iniziali (tutti a zero — primo esercizio)
        var allUnitIds = apIds.Values.Concat(boxIds.Values).ToList();
        var bulkBalanceDto = new SetUnitOpeningBalancesBulkDto
        {
            FiscalYearId = fyId,
            Items        = allUnitIds.Select(uid => new UnitOpeningBalanceItemDto
            {
                UnitId         = uid,
                OpeningBalance = 0m,
            }).ToList(),
        };
        var balResp = await Client.PutAsJsonAsync("/api/real-estate-units/opening-balances/bulk", bulkBalanceDto);
        balResp.StatusCode.Should().Be(HttpStatusCode.NoContent, await ReadErrorAsync(balResp));
        output.WriteLine($"  Saldi iniziali impostati (0 € × {allUnitIds.Count} unità).");

        // Apertura esercizio
        var openResp = await Client.PostAsJsonAsync($"/api/fiscal-years/{fyId}/open", new { });
        openResp.StatusCode.Should().Be(HttpStatusCode.NoContent, await ReadErrorAsync(openResp));
        output.WriteLine("  Esercizio 2024 aperto.");

        // ── 7. Budget Preventivo 2024 (12.400 €) ─────────────────────────────
        var budPrevDto = new CreateBudgetDto
        {
            CondominiumId = condId,
            FiscalYearId  = fyId,
            Type          = BudgetType.Preventivo,
            Notes         = "Preventivo 2024 approvato assemblea 15/01/2024",
        };
        var (budPrevResp, budPrev) = await PostAsync<BudgetReadDto>("/api/budgets", budPrevDto);
        budPrevResp.StatusCode.Should().Be(HttpStatusCode.Created, await ReadErrorAsync(budPrevResp));
        var budPrevId = budPrev!.Id;
        output.WriteLine($"  Budget preventivo creato: id={budPrevId}");

        // Voci budget preventivo
        var prevItems = new[]
        {
            (accManutenzione,    "Manutenzione ordinaria parti comuni",   3200.00m),
            (accPulizie,         "Servizio pulizie scale e aree comuni",  2400.00m),
            (accAmministrazione, "Onorario amministratore",               1800.00m),
            (accAssicurazione,   "Assicurazione fabbricato",              1400.00m),
            (accUtenze,          "Utenze (luce scale, acqua)",            2200.00m),
            (accFondoRiserva,    "Fondo riserva",                         1400.00m),
        };
        foreach (var (accountId, description, amount) in prevItems)
        {
            var dto = new CreateBudgetItemDto
            {
                BudgetId    = budPrevId,
                AccountId   = accountId,
                Description = description,
                Amount      = amount,
            };
            var (r, _) = await PostAsync<BudgetItemReadDto>("/api/budget-items", dto);
            r.StatusCode.Should().Be(HttpStatusCode.Created, $"voce preventivo: {description}");
        }
        output.WriteLine("  Voci preventivo inserite (6).");

        // Approvazione preventivo
        var approvaPrevResp = await Client.PostAsJsonAsync($"/api/budgets/{budPrevId}/approve", new { });
        approvaPrevResp.StatusCode.Should().Be(HttpStatusCode.NoContent, await ReadErrorAsync(approvaPrevResp));
        output.WriteLine("  Preventivo 2024 approvato.");

        // ── 8. Budget Consuntivo 2024 (12.850 €) ─────────────────────────────
        var budConsDto = new CreateBudgetDto
        {
            CondominiumId = condId,
            FiscalYearId  = fyId,
            Type          = BudgetType.Consuntivo,
            Notes         = "Consuntivo 2024 approvato assemblea 20/01/2025",
        };
        var (budConsResp, budCons) = await PostAsync<BudgetReadDto>("/api/budgets", budConsDto);
        budConsResp.StatusCode.Should().Be(HttpStatusCode.Created, await ReadErrorAsync(budConsResp));
        var budConsId = budCons!.Id;
        output.WriteLine($"  Budget consuntivo creato: id={budConsId}");

        // Voci consuntivo
        var consItems = new[]
        {
            (accManutenzione,    "Manutenzione ordinaria parti comuni",   3450.00m, "Intervento extra cancello automatico"),
            (accPulizie,         "Servizio pulizie scale e aree comuni",  2400.00m, (string?)null),
            (accAmministrazione, "Onorario amministratore",               1800.00m, (string?)null),
            (accAssicurazione,   "Assicurazione fabbricato",              1420.00m, "Adeguamento premio"),
            (accUtenze,          "Utenze (luce scale, acqua)",            2380.00m, (string?)null),
            (accFondoRiserva,    "Fondo riserva",                         1400.00m, (string?)null),
        };
        foreach (var (accountId, description, amount, notes) in consItems)
        {
            var dto = new CreateBudgetItemDto
            {
                BudgetId    = budConsId,
                AccountId   = accountId,
                Description = description,
                Amount      = amount,
                Notes       = notes,
            };
            var (r, _) = await PostAsync<BudgetItemReadDto>("/api/budget-items", dto);
            r.StatusCode.Should().Be(HttpStatusCode.Created, $"voce consuntivo: {description}");
        }
        output.WriteLine("  Voci consuntivo inserite (6).");

        // Approvazione consuntivo
        var approvaConsResp = await Client.PostAsJsonAsync($"/api/budgets/{budConsId}/approve", new { });
        approvaConsResp.StatusCode.Should().Be(HttpStatusCode.NoContent, await ReadErrorAsync(approvaConsResp));
        output.WriteLine("  Consuntivo 2024 approvato.");

        // ── 9. Rate e quote ────────────────────────────────────────────────────
        // Le rate e le fee per unità vengono generate automaticamente dall'approvazione
        // del preventivo (ApproveBudgetCommandConsumer). Non servono passi aggiuntivi.
        output.WriteLine("  Rate e fee generate automaticamente dall'approvazione preventivo.");

        // ── 10. Spese reali 2024 ─────────────────────────────────────────────
        var suppliers = new Dictionary<string, int>();

        // Crea fornitori
        var supplierData = new[]
        {
            ("TECNO-IMPL",  "Tecno Impianti Srl",       "IT03456780152"),
            ("CLEANPRO",    "CleanPro Srl",              "IT07891230154"),
            ("STUD-FERR",   "Studio Amm. Ferretti",      "IT09876540151"),
        };
        foreach (var (code, name, vat) in supplierData)
        {
            var dto = new CreateSupplierDto
            {
                CompanyName = name,
                VatNumber   = vat,
                IsActive    = true,
            };
            var (r, sup) = await PostAsync<SupplierReadDto>("/api/suppliers", dto);
            if (r.StatusCode == HttpStatusCode.Created)
                suppliers[code] = sup!.Id;
        }
        output.WriteLine($"  Fornitori creati: {suppliers.Count}");

        // Spese
        // (accountId, supplierId_key, name, taxable, vat, expenseTypeId, docDate, regDate)
        var spese = new[]
        {
            (accManutenzione,    "TECNO-IMPL", "Manutenzione cancello automatico",  1025.41m, 224.59m, ExpenseType.Manutenzione, new DateTime(2024,  3, 10), new DateTime(2024,  3, 12)),
            (accManutenzione,    "TECNO-IMPL", "Riparazione citofoni scala B",        475.41m, 104.59m, ExpenseType.Manutenzione, new DateTime(2024,  5, 20), new DateTime(2024,  5, 22)),
            (accPulizie,         "CLEANPRO",   "Servizio pulizie annuale",           1967.21m, 432.79m, ExpenseType.Pulizie,      new DateTime(2024, 12, 31), new DateTime(2024, 12, 31)),
            (accAmministrazione, "STUD-FERR",  "Onorario amministratore 2024",       1639.34m, 160.66m, ExpenseType.Professionale,new DateTime(2024, 12, 31), new DateTime(2024, 12, 31)),
            (accAssicurazione,   (string?)null,"Assicurazione fabbricato 2024",      1420.00m,   0.00m, ExpenseType.Altro,        new DateTime(2024,  1, 15), new DateTime(2024,  1, 16)),
            (accUtenze,          (string?)null,"Bollette energia elettrica scale",   1950.82m, 429.18m, ExpenseType.Utenze,       new DateTime(2024, 12, 15), new DateTime(2024, 12, 16)),
        };

        foreach (var (accountId, supKey, name, taxable, vat, expenseTypeId, docDate, regDate) in spese)
        {
            var dto = new CreateExpenseDto
            {
                CondominiumId          = condId,
                FiscalYearId           = fyId,
                AccountId              = accountId,
                MillesimalTableId      = tmgTable.Id,
                SupplierId             = supKey != null && suppliers.ContainsKey(supKey) ? suppliers[supKey] : (int?)null,
                Name                   = name,
                TaxableAmount          = taxable,
                VatAmount              = vat,
                ExpenseTypeId          = expenseTypeId,
                DocumentDate           = docDate,
                RegistrationDate       = regDate,
                ChargeabilityTypeId    = ChargeabilityType.Owner,
            };
            var (r, _) = await PostAsync<ExpenseReadDto>("/api/expenses", dto);
            r.StatusCode.Should().Be(HttpStatusCode.Created, $"spesa: {name}");
        }
        output.WriteLine("  Spese create (6).");

        // ── Output riepilogo ──────────────────────────────────────────────────
        output.WriteLine("");
        output.WriteLine("  ✓ Seed completato.");
        output.WriteLine($"    CondominiumId  = {condId}");
        output.WriteLine($"    FiscalYearId   = {fyId}");
        output.WriteLine($"    BudgetPrevId   = {budPrevId}");
        output.WriteLine($"    BudgetConsId   = {budConsId}");
        output.WriteLine($"    Tabella TMG    = {tmgTable.Id}");
        output.WriteLine($"    Proprietari    = 12 (9 apt + 3 box)");
        output.WriteLine($"    Spese          = 6");
        output.WriteLine($"    Morosità       = Galli B01 (Q4) + Greco C03 (Q3+Q4)");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<MillesimalTableReadDto> CreateMillesimalTableAsync(
        int condId, string code, string name, decimal total)
    {
        var existingResp = await Client.GetAsync(
            $"/api/millesimal-tables/by-condominium/{condId}/code/{code}");
        if (existingResp.StatusCode == HttpStatusCode.OK)
        {
            var existing = await existingResp.Content
                .ReadFromJsonAsync<MillesimalTableReadDto>(JsonOptions);
            if (existing != null) return existing;
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
                r.StatusCode.Should().Be(HttpStatusCode.Created,
                    $"millesimale app {apKeys[i]} tab {tableId}");
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
                r.StatusCode.Should().Be(HttpStatusCode.Created,
                    $"millesimale box {boxKeys[i]} tab {tableId}");
            }
        }
    }
}
