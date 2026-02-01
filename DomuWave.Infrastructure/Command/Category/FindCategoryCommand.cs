using DomuWave.Services.Models.Dto.Category;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Category;

public class FindCategoryCommand : BaseBookRelatedCommand, IQuery<IList<CategoryReadDto>>
{
    public FindCategoryCommand()
    {
    }
    public FindCategoryCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
    {

    }
    public string Q { get; set; }
}