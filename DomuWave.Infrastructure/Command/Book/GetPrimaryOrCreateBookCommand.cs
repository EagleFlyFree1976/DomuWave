namespace DomuWave.Services.Command.Book;

public class GetPrimaryOrCreateBookCommand : CreateBookCommand
{
    public GetPrimaryOrCreateBookCommand()
    {
    }

    public GetPrimaryOrCreateBookCommand(int currentUserId) : base(currentUserId)
    {
    }
}