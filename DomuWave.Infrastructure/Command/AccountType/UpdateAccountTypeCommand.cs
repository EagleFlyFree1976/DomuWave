using DomuWave.Services.Models.Dto;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

/// <summary>
/// Aggiorna una currency esistente
/// </summary>
public class UpdateAccountTypeCommand : BaseCommand,IQuery<AccountTypeReadDto>
{
    public UpdateAccountTypeCommand()
    {
    }

    public UpdateAccountTypeCommand(int currentUserId) : base(currentUserId)
    {
    }

    public int AccountTypeId { get; set; }
    public AccountTypeCreateUpdateDto UpdateDto { get; set; }
        
        
}