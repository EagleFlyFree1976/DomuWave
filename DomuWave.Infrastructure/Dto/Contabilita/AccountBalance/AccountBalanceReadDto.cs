namespace DomuWave.Services.Dto.Contabilita.AccountBalance;

public class AccountBalanceReadDto
{
    public int     Id                      { get; set; }
    public int     FiscalYearId            { get; set; }
    public int     AccountId               { get; set; }
    public string  AccountCode             { get; set; } = string.Empty;
    public string  AccountName             { get; set; } = string.Empty;
    public int     AccountType             { get; set; }
    public string  AccountTypeLabel        { get; set; } = string.Empty;
    /// <summary>Saldo iniziale (SaldoIniziale).</summary>
    public decimal OpeningBalance          { get; set; }
    /// <summary>Totale movimenti dell'esercizio per questo conto.</summary>
    public decimal MovementsTotal          { get; set; }
    /// <summary>Saldo corrente = SaldoIniziale ± movimenti (SaldoTotale).</summary>
    public decimal TotalBalance            { get; set; }
    /// <summary>Saldo finale generato alla chiusura (SaldoFinale). 0 finché non chiuso.</summary>
    public decimal ClosingBalance          { get; set; }
    /// <summary>True se il saldo iniziale è modificabile (primo esercizio + non chiuso).</summary>
    public bool    IsOpeningBalanceEditable  { get; set; }
    /// <summary>Somma degli arrotondamenti applicati alle ripartizioni spese di questo conto nell'esercizio.</summary>
    public decimal TotalRoundingAdjustment   { get; set; }
}
