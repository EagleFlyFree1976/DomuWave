namespace DomuWave.Services.Models;

public class Category : AccountEntity<long>
{
    public virtual Category ParentCategory { get; set; }
    public override int GetHashCode()
    {
        return this.Id.GetHashCode();
    }
}