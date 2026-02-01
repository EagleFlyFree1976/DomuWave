using DomuWave.Services.Models.Dto;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

public class ResetAccountCommand : BaseBookRelatedCommand,IQuery<bool>
{
    public ResetAccountCommand()
    {
    }
    public ResetAccountCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
    {
    }


    public long AccountId { get; set; }
}