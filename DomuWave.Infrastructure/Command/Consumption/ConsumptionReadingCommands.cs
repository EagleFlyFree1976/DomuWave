using DomuWave.Services.Dto.Consumption;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Consumption;

public class GetConsumptionReadingsByFiscalYearCommand : BaseCommand, IQuery<IList<ConsumptionReadingReadDto>>
{
    public int ConsumptionTypeId { get; set; }
    public int FiscalYearId      { get; set; }
    public GetConsumptionReadingsByFiscalYearCommand() { }
    public GetConsumptionReadingsByFiscalYearCommand(int currentUserId, int consumptionTypeId, int fiscalYearId) : base(currentUserId)
    { ConsumptionTypeId = consumptionTypeId; FiscalYearId = fiscalYearId; }
}

public class GetConsumptionReadingByIdCommand : BaseCommand, IQuery<ConsumptionReadingReadDto>
{
    public int Id { get; set; }
    public GetConsumptionReadingByIdCommand() { }
    public GetConsumptionReadingByIdCommand(int currentUserId, int id) : base(currentUserId) => Id = id;
}

public class SaveConsumptionReadingsBulkCommand : BaseCommand, IQuery<IList<ConsumptionReadingReadDto>>
{
    public int                             ConsumptionTypeId { get; set; }
    public int                             FiscalYearId      { get; set; }
    public IList<UpsertConsumptionReadingDto> Items          { get; set; } = [];
    public SaveConsumptionReadingsBulkCommand() { }
    public SaveConsumptionReadingsBulkCommand(int currentUserId, int consumptionTypeId, int fiscalYearId, IList<UpsertConsumptionReadingDto> items) : base(currentUserId)
    { ConsumptionTypeId = consumptionTypeId; FiscalYearId = fiscalYearId; Items = items; }
}

public class UpsertConsumptionReadingDto
{
    public int       MeterId      { get; set; }
    public DateTime? InitialDate  { get; set; }
    public decimal   InitialValue { get; set; }
    public DateTime? FinalDate    { get; set; }
    public decimal   FinalValue   { get; set; }
    public string?   Notes        { get; set; }
}

public class DeleteConsumptionReadingCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }
    public DeleteConsumptionReadingCommand() { }
    public DeleteConsumptionReadingCommand(int currentUserId, int id) : base(currentUserId) => Id = id;
}
