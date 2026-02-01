using DomuWave.Services.Models.Dto.Category;
using DomuWave.Services.Models.Dto.Category;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Category
{
    /// <summary>
    /// Crea un nuovo metodo di pagamento con im parametri impostati
    /// </summary>
    public class CreateCategoryCommand : BaseBookRelatedCommand,IQuery<CategoryReadDto>
    {
        public CreateCategoryCommand()
        {
        }

        public CreateCategoryCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
        {
        }

        public CategoryCreateUpdateDto CreateDto { get; set; }
        
    }
}
