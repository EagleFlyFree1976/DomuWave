namespace DomuWave.Services.Models;

/// <summary>
/// Template standard del piano dei conti italiano applicato automaticamente
/// a ogni nuovo condominio e disponibile come comando manuale.
/// </summary>
public static class ChartOfAccountsDefaultTemplate
{
    /// <summary>(Code, Name, Type, ParentCode)</summary>
    public static readonly (string Code, string Name, ChartOfAccountsType Type, string? ParentCode)[] Items =
    [
        // ── USCITE ───────────────────────────────────────────────────────────
        ("U",    "Uscite",                             ChartOfAccountsType.Uscita,       null),
        ("U.01", "Pulizie e igiene",                   ChartOfAccountsType.Uscita,       "U"),
        ("U.02", "Giardinaggio e aree verdi",          ChartOfAccountsType.Uscita,       "U"),
        ("U.03", "Manutenzione ordinaria",             ChartOfAccountsType.Uscita,       "U"),
        ("U.04", "Manutenzione straordinaria",         ChartOfAccountsType.Uscita,       "U"),
        ("U.05", "Ascensore",                          ChartOfAccountsType.Uscita,       "U"),
        ("U.06", "Riscaldamento centralizzato",        ChartOfAccountsType.Uscita,       "U"),
        ("U.07", "Acqua comune",                       ChartOfAccountsType.Uscita,       "U"),
        ("U.08", "Energia elettrica parti comuni",     ChartOfAccountsType.Uscita,       "U"),
        ("U.09", "Gas parti comuni",                   ChartOfAccountsType.Uscita,       "U"),
        ("U.10", "Portineria e vigilanza",             ChartOfAccountsType.Uscita,       "U"),
        ("U.11", "Assicurazioni",                      ChartOfAccountsType.Uscita,       "U"),
        ("U.12", "Onorario amministratore",            ChartOfAccountsType.Uscita,       "U"),
        ("U.13", "Spese legali e consulenze",          ChartOfAccountsType.Uscita,       "U"),
        ("U.14", "Spese bancarie e postali",           ChartOfAccountsType.Uscita,       "U"),
        ("U.15", "Imposte e tasse",                    ChartOfAccountsType.Uscita,       "U"),
        ("U.16", "Fornitura materiali",                ChartOfAccountsType.Uscita,       "U"),
        ("U.17", "Lavori strutturali",                 ChartOfAccountsType.Uscita,       "U"),
        ("U.18", "Impianti tecnologici",               ChartOfAccountsType.Uscita,       "U"),
        ("U.19", "Fondo di riserva",                   ChartOfAccountsType.Uscita,       "U"),
        ("U.20", "Spese varie",                        ChartOfAccountsType.Uscita,       "U"),

        // ── ENTRATE ──────────────────────────────────────────────────────────
        ("E",    "Entrate",                            ChartOfAccountsType.Entrata,      null),
        ("E.01", "Quote condominiali ordinarie",       ChartOfAccountsType.Entrata,      "E"),
        ("E.02", "Quote straordinarie",                ChartOfAccountsType.Entrata,      "E"),
        ("E.03", "Interessi attivi",                   ChartOfAccountsType.Entrata,      "E"),
        ("E.04", "Proventi da locazione spazi comuni", ChartOfAccountsType.Entrata,      "E"),
        ("E.05", "Rimborsi assicurativi",              ChartOfAccountsType.Entrata,      "E"),
        ("E.06", "Entrate varie",                      ChartOfAccountsType.Entrata,      "E"),

        // ── PATRIMONIALE ─────────────────────────────────────────────────────
        ("P",    "Patrimoniale",                       ChartOfAccountsType.Patrimoniale, null),
        ("P.01", "Fondo cassa",                        ChartOfAccountsType.Patrimoniale, "P"),
        ("P.02", "Conto corrente bancario",            ChartOfAccountsType.Patrimoniale, "P"),
        ("P.03", "Crediti verso condomini",            ChartOfAccountsType.Patrimoniale, "P"),
        ("P.04", "Debiti verso fornitori",             ChartOfAccountsType.Patrimoniale, "P"),
        ("P.05", "Fondo di riserva (patrimonio)",      ChartOfAccountsType.Patrimoniale, "P"),
    ];
}
