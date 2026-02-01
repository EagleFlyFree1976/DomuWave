namespace DomuWave.Services.Models;

public interface IownerEntity
{
      int OwnerId { get; set; }
}


public abstract class BookEntity<T> : GenericEntity<T>
{
    public virtual Book Book { get; set; } 
}

public abstract class AccountEntity<T> : BookEntity<T>
{
    public virtual Account Account { get; set; }
}