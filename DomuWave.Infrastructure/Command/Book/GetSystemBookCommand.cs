using DomuWave.Services.Models.Dto;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

public class GetSystemBookCommand : BaseCommand, IQuery<BookReadDto>
{
    public GetSystemBookCommand()
    {
    }

 
}