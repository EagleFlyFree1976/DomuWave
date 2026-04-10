using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.Consumption;

public class ConsumptionTypeReadDto : TraceEntityDTO<int>
{
    public int    CondominiumId   { get; set; }
    public string CondominiumName { get; set; }
    public string Name            { get; set; }
    public string UnitOfMeasure   { get; set; }
    public string Notes           { get; set; }
    public bool   IsActive        { get; set; }
    public bool   IsDeleted       { get; set; }
}

public class CreateConsumptionTypeDto
{
    public int    CondominiumId { get; set; }
    public string Name          { get; set; }
    public string UnitOfMeasure { get; set; }
    public string Notes         { get; set; }
}

public class UpdateConsumptionTypeDto
{
    public string  Name          { get; set; }
    public string  UnitOfMeasure { get; set; }
    public string? Notes         { get; set; }
    public bool    IsActive      { get; set; }
}
