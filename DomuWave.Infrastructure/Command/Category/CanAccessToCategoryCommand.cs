using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Category;

public class CanAccessToCategoryCommand : BaseBookRelatedCommand, IQuery<bool>
{
    public long? CategoryId { get; set; }
    public CanAccessToCategoryCommand()
    {
    }

    public CanAccessToCategoryCommand(long? categoryId, int currentUserId, long bookId) : base(currentUserId, bookId)
    {
        CategoryId = categoryId;
    }
}