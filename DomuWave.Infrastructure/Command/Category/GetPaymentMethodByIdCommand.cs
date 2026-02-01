using DomuWave.Services.Models.Dto;
using DomuWave.Services.Models.Dto.Category;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Category;

/// <summary>
///  Ritorna il dettaglio dela categoria selezionata
/// </summary>
public class GetCategoryByIdCommand : BaseBookRelatedCommand, IQuery<CategoryReadDto>
{
    public GetCategoryByIdCommand()
    {
    }

    public GetCategoryByIdCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
    {
        
    }
   
    public long CategoryId { get; set; }
    
}