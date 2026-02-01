using DomuWave.Services.Models.Dto.Category;
using DomuWave.Services.Models.Dto.Category;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Category;

public class UpdateCategoryCommand : BaseBookRelatedCommand, IQuery<CategoryReadDto>
{
    public UpdateCategoryCommand()
    {
    }
    public long CategoryId { get; set; }

    public UpdateCategoryCommand(long categoryId, int currentUserId, long bookId) : base(currentUserId, bookId)
    {
        CategoryId = categoryId;
    }

    public CategoryCreateUpdateDto UpdateDto { get; set; }

}