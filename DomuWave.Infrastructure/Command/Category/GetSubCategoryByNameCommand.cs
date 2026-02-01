using DomuWave.Services.Models.Dto.Category;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Category;

public class GetSubCategoryByNameCommand : BaseBookRelatedCommand, IQuery<CategoryReadDto>
{
    public GetSubCategoryByNameCommand()
    {
    }
    public GetSubCategoryByNameCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
    {

    }

    public long ParentCategoryId { get; set; }
    public string Name { get; set; }
}