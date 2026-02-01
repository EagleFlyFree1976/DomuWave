using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

/// <summary>
///  Imposta il book specificato come primario
/// </summary>
public class SetBookaAsPrimaryCommand : BaseBookRelatedCommand, IQuery<bool>
{
    public SetBookaAsPrimaryCommand()
    {
    }

    public SetBookaAsPrimaryCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
    {
    }

    public int BookId { get; set; }
}