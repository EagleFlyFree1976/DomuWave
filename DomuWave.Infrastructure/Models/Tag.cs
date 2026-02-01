namespace DomuWave.Services.Models;

public class Tag : BookEntity<long>
{
    public override int GetHashCode()
    {
        return this.Id.GetHashCode();
    }
    
}