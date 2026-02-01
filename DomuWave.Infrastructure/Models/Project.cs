namespace DomuWave.Services.Models;

public class Project : BookEntity<long>
{
    
    public virtual IList<Transaction> Transactions { get; set; }
    public override int GetHashCode()
    {
        return this.Id.GetHashCode();
    }
}