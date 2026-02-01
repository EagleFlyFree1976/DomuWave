using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Category;

public class DeleteCategoryByIdCommand : BaseBookRelatedCommand, IQuery<bool>
{
    public DeleteCategoryByIdCommand()
    {
    }

    public DeleteCategoryByIdCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
    {
    }


    public long CategoryId { get; set; }
}