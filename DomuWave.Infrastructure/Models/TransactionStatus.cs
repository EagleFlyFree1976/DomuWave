namespace DomuWave.Services.Models;

public class TransactionStatus : GenericEntity<int>
{
    public virtual string Code { get; set; }
    public virtual string CssClass { get; set; }
    public virtual bool IncludeInBalance { get; set; }
    public override int GetHashCode()
    {

        
        return this.Id.GetHashCode();
    }
}