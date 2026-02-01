namespace DomuWave.Services.Command;

public abstract class BaseBookRelatedCommand : BaseCommand
{
    
    

    public long BookId { get; set; }

    protected BaseBookRelatedCommand()
    {
    }


    protected BaseBookRelatedCommand(int currentUserId, long bookId) : base(currentUserId)
    {
        BookId = bookId;
    }
}

