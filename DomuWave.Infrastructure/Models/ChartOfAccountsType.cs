namespace DomuWave.Services.Models;

public enum ChartOfAccountsType
{
    /// <summary>Voce di entrata (incassi, quote, rimborsi).</summary>
    Entrata = 1,

    /// <summary>Voce di uscita (spese, compensi, forniture).</summary>
    Uscita = 2,

    /// <summary>Voce patrimoniale (fondi, riserve, conti di deposito).</summary>
    Patrimoniale = 3
}
