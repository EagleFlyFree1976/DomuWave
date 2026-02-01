using DomuWave.Services.Models.Dto.Category;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Category;

public class GetCategoryByNameCommand : BaseBookRelatedCommand, IQuery<CategoryReadDto>
{
    public GetCategoryByNameCommand()
    {
    }
    public GetCategoryByNameCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
    {

    }
    public string Name { get; set; }
}