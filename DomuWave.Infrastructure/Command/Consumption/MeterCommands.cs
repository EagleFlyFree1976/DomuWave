using DomuWave.Services.Dto.Consumption;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Consumption;

public class GetMetersByCondominiumCommand : BaseCommand, IQuery<IList<MeterReadDto>>
{
    public int CondominiumId { get; set; }
    public GetMetersByCondominiumCommand() { }
    public GetMetersByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
        => CondominiumId = condominiumId;
}

public class GetMetersByConsumptionTypeCommand : BaseCommand, IQuery<IList<MeterReadDto>>
{
    public int ConsumptionTypeId { get; set; }
    public GetMetersByConsumptionTypeCommand() { }
    public GetMetersByConsumptionTypeCommand(int currentUserId, int consumptionTypeId) : base(currentUserId)
        => ConsumptionTypeId = consumptionTypeId;
}

public class GetMeterByIdCommand : BaseCommand, IQuery<MeterReadDto>
{
    public int Id { get; set; }
    public GetMeterByIdCommand() { }
    public GetMeterByIdCommand(int currentUserId, int id) : base(currentUserId) => Id = id;
}

public class CreateMeterCommand : BaseCommand, IQuery<MeterReadDto>
{
    public CreateMeterDto Dto { get; set; }
    public CreateMeterCommand() { }
    public CreateMeterCommand(int currentUserId, CreateMeterDto dto) : base(currentUserId) => Dto = dto;
}

public class UpdateMeterCommand : BaseCommand, IQuery<MeterReadDto>
{
    public int Id { get; set; }
    public UpdateMeterDto Dto { get; set; }
    public UpdateMeterCommand() { }
    public UpdateMeterCommand(int currentUserId, int id, UpdateMeterDto dto) : base(currentUserId) { Id = id; Dto = dto; }
}

public class DeleteMeterCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }
    public DeleteMeterCommand() { }
    public DeleteMeterCommand(int currentUserId, int id) : base(currentUserId) => Id = id;
}
