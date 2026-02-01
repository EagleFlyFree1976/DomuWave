using DomuWave.Services.Models.Dto;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

public class UpdateAccountCommand : BaseBookRelatedCommand,IQuery<AccountReadDto>
{
    public UpdateAccountCommand()
    {
    }

    public UpdateAccountCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
    {
    }

    public AccountUpdateDto UpdateDto { get; set; }
        
}