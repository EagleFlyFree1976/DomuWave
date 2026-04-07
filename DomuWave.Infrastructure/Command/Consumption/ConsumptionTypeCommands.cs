using DomuWave.Services.Dto.Consumption;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Consumption;

public class GetConsumptionTypesByCondominiumCommand : BaseCommand, IQuery<IList<ConsumptionTypeReadDto>>
{
    public int CondominiumId { get; set; }
    public GetConsumptionTypesByCondominiumCommand() { }
    public GetConsumptionTypesByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
        => CondominiumId = condominiumId;
}

public class GetConsumptionTypeByIdCommand : BaseCommand, IQuery<ConsumptionTypeReadDto>
{
    public int Id { get; set; }
    public GetConsumptionTypeByIdCommand() { }
    public GetConsumptionTypeByIdCommand(int currentUserId, int id) : base(currentUserId) => Id = id;
}

public class CreateConsumptionTypeCommand : BaseCommand, IQuery<ConsumptionTypeReadDto>
{
    public CreateConsumptionTypeDto Dto { get; set; }
    public CreateConsumptionTypeCommand() { }
    public CreateConsumptionTypeCommand(int currentUserId, CreateConsumptionTypeDto dto) : base(currentUserId) => Dto = dto;
}

public class UpdateConsumptionTypeCommand : BaseCommand, IQuery<ConsumptionTypeReadDto>
{
    public int Id { get; set; }
    public UpdateConsumptionTypeDto Dto { get; set; }
    public UpdateConsumptionTypeCommand() { }
    public UpdateConsumptionTypeCommand(int currentUserId, int id, UpdateConsumptionTypeDto dto) : base(currentUserId) { Id = id; Dto = dto; }
}

public class DeleteConsumptionTypeCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }
    public DeleteConsumptionTypeCommand() { }
    public DeleteConsumptionTypeCommand(int currentUserId, int id) : base(currentUserId) => Id = id;
}
