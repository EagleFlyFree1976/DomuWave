namespace DomuWave.Services.Models;

public class AccountBalance : TenantEntity<int>
{
    public virtual FiscalYear      FiscalYear     { get; set; } = null!;
    public virtual ChartOfAccounts Account        { get; set; } = null!;
    /// <summary>Saldo iniziale dell'esercizio per questo conto.</summary>
    public virtual decimal         OpeningBalance { get; set; } = 0;
    /// <summary>Saldo finale generato alla chiusura dell'esercizio (0 finché non chiuso).</summary>
    public virtual decimal         ClosingBalance            { get; set; } = 0;
    /// <summary>Saldo totale = OpeningBalance + movimenti con segno (aggiornato alla chiusura).</summary>
    public virtual decimal         TotalBalance              { get; set; } = 0;
    /// <summary>Somma degli arrotondamenti applicati alle ripartizioni spese di questo conto.</summary>
    public virtual decimal         TotalRoundingAdjustment   { get; set; } = 0;

    public override int GetHashCode() => Id.GetHashCode();
}
