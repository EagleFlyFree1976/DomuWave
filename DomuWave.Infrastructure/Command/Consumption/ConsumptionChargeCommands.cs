using DomuWave.Services.Dto.Consumption;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Consumption;

public class GetConsumptionChargesByFiscalYearCommand : BaseCommand, IQuery<IList<ConsumptionChargeReadDto>>
{
    public int FiscalYearId { get; set; }
    public GetConsumptionChargesByFiscalYearCommand() { }
    public GetConsumptionChargesByFiscalYearCommand(int currentUserId, int fiscalYearId) : base(currentUserId)
        => FiscalYearId = fiscalYearId;
}

public class GetConsumptionChargeByIdCommand : BaseCommand, IQuery<ConsumptionChargeReadDto>
{
    public int Id { get; set; }
    public GetConsumptionChargeByIdCommand() { }
    public GetConsumptionChargeByIdCommand(int currentUserId, int id) : base(currentUserId) => Id = id;
}

public class CreateConsumptionChargeCommand : BaseCommand, IQuery<ConsumptionChargeReadDto>
{
    public CreateConsumptionChargeDto Dto { get; set; }
    public CreateConsumptionChargeCommand() { }
    public CreateConsumptionChargeCommand(int currentUserId, CreateConsumptionChargeDto dto) : base(currentUserId) => Dto = dto;
}

public class UpdateConsumptionChargeCommand : BaseCommand, IQuery<ConsumptionChargeReadDto>
{
    public int Id { get; set; }
    public UpdateConsumptionChargeDto Dto { get; set; }
    public UpdateConsumptionChargeCommand() { }
    public UpdateConsumptionChargeCommand(int currentUserId, int id, UpdateConsumptionChargeDto dto) : base(currentUserId) { Id = id; Dto = dto; }
}

public class RecalculateConsumptionChargeCommand : BaseCommand, IQuery<ConsumptionChargeReadDto>
{
    public int Id { get; set; }
    public RecalculateConsumptionChargeCommand() { }
    public RecalculateConsumptionChargeCommand(int currentUserId, int id) : base(currentUserId) => Id = id;
}

public class ApproveConsumptionChargeCommand : BaseCommand, IQuery<ConsumptionChargeReadDto>
{
    public int Id { get; set; }
    public ApproveConsumptionChargeCommand() { }
    public ApproveConsumptionChargeCommand(int currentUserId, int id) : base(currentUserId) => Id = id;
}

public class DeleteConsumptionChargeCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }
    public DeleteConsumptionChargeCommand() { }
    public DeleteConsumptionChargeCommand(int currentUserId, int id) : base(currentUserId) => Id = id;
}
