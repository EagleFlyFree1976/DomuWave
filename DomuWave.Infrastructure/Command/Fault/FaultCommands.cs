using DomuWave.Services.Dto.Fault;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Fault;

public class GetFaultsByCondominiumCommand : BaseCommand, IQuery<IList<FaultReadDto>>
{
    public int CondominiumId { get; set; }
    public GetFaultsByCondominiumCommand() { }
    public GetFaultsByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
        => CondominiumId = condominiumId;
}

public class GetFaultByIdCommand : BaseCommand, IQuery<FaultReadDto>
{
    public int Id { get; set; }
    public GetFaultByIdCommand() { }
    public GetFaultByIdCommand(int currentUserId, int id) : base(currentUserId) => Id = id;
}

public class CreateFaultCommand : BaseCommand, IQuery<FaultReadDto>
{
    public CreateFaultDto Dto { get; set; } = null!;
    public CreateFaultCommand() { }
    public CreateFaultCommand(int currentUserId, CreateFaultDto dto) : base(currentUserId) => Dto = dto;
}

public class UpdateFaultStatusCommand : BaseCommand, IQuery<FaultReadDto>
{
    public int                  Id  { get; set; }
    public UpdateFaultStatusDto Dto { get; set; } = null!;
    public UpdateFaultStatusCommand() { }
    public UpdateFaultStatusCommand(int currentUserId, int id, UpdateFaultStatusDto dto) : base(currentUserId) { Id = id; Dto = dto; }
}

public class DeleteFaultCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }
    public DeleteFaultCommand() { }
    public DeleteFaultCommand(int currentUserId, int id) : base(currentUserId) => Id = id;
}

public class GetFaultMessagesByFaultCommand : BaseCommand, IQuery<IList<FaultMessageReadDto>>
{
    public int FaultId { get; set; }
    public GetFaultMessagesByFaultCommand() { }
    public GetFaultMessagesByFaultCommand(int currentUserId, int faultId) : base(currentUserId) => FaultId = faultId;
}

public class CreateFaultMessageCommand : BaseCommand, IQuery<FaultMessageReadDto>
{
    public CreateFaultMessageDto Dto { get; set; } = null!;
    public CreateFaultMessageCommand() { }
    public CreateFaultMessageCommand(int currentUserId, CreateFaultMessageDto dto) : base(currentUserId) => Dto = dto;
}
