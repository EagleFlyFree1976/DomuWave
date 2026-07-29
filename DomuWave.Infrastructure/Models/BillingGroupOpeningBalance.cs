namespace DomuWave.Services.Models;

/// <summary>
/// Bilancio (apertura + chiusura) di un GRUPPO DI FATTURAZIONE per un esercizio.
/// Analogo a UnitOpeningBalance, ma a livello di BillingGroup: quando più unità
/// condividono lo stesso gruppo di fatturazione, il saldo iniziale/finale viene
/// gestito una sola volta sul gruppo (mai spalmato sulle singole unità componenti).
///
/// RateAddebitate/RateIncassate aggregano i movimenti (CondominiumFee) di TUTTE
/// le unità del gruppo, con lo stesso criterio già usato per il conguaglio.
///
/// Regole di editabilità: identiche a UnitOpeningBalance (vedi quella classe).
/// </summary>
public class BillingGroupOpeningBalance : TenantEntity<int>
{
    public virtual BillingGroup   BillingGroup { get; set; }
    public virtual FiscalYear     FiscalYear   { get; set; }

    public virtual decimal OpeningBalance   { get; set; } = 0;
    public virtual decimal RateAddebitate   { get; set; } = 0;
    public virtual decimal RateIncassate    { get; set; } = 0;
    public virtual decimal QuotaConsuntiva  { get; set; } = 0;
    public virtual decimal SaldoConguaglio  { get; set; } = 0;
    public virtual decimal TotalMovements   { get; set; } = 0;
    public virtual decimal ClosingBalance   { get; set; } = 0;

    public virtual string  Notes            { get; set; }

    public override int GetHashCode() => Id.GetHashCode();
}
