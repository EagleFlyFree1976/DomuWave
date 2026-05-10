using DomuWave.Services.Dto.Staircase;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Staircase;

public class GetStaircaseByIdCommand : BaseCommand, IQuery<StaircaseReadDto>
{
    public int Id { get; set; }

    public GetStaircaseByIdCommand() { }
    public GetStaircaseByIdCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}
