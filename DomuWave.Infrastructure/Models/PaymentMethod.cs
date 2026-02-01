namespace DomuWave.Services.Models;

/// <summary>
/// Metodo di pagamento disponibili
/// </summary>
public class PaymentMethod : BookEntity<int>
{

    public virtual bool IsEnabled { get; set; }
    public override int GetHashCode()
    {
        return this.Id.GetHashCode();
    }
}