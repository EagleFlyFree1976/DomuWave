namespace DomuWave.Services.Models;

public class Beneficiary : BookEntity<long>
{
    public virtual string Iban { get; set; }
    public virtual string Notes { get; set; }

    public virtual Category Category { get; set; }


    public override int GetHashCode()
    {
        return this.Id.GetHashCode();
    }
}