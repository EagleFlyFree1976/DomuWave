using DomuWave.Services.Models.Dto;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

public class GetAccountTypeByIdCommand : BaseCommand, IQuery<AccountTypeReadDto>
{
    public GetAccountTypeByIdCommand()
    {
    }

    public GetAccountTypeByIdCommand(int currentUserId) : base(currentUserId)
    {
    }

    public int Id { get; set; }
}