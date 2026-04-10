using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.Consumption;

public class MeterReadDto : TraceEntityDTO<int>
{
    public int    ConsumptionTypeId        { get; set; }
    public string ConsumptionTypeName      { get; set; }
    public bool   ConsumptionTypeIsDeleted { get; set; }
    public string UnitOfMeasure            { get; set; }
    public int    UnitId              { get; set; }
    public string UnitName            { get; set; }
    public string Code                { get; set; }
    public string Notes               { get; set; }
    public bool   IsActive            { get; set; }
    public bool   IsDeleted           { get; set; }
}

public class CreateMeterDto
{
    public int    ConsumptionTypeId { get; set; }
    public int    UnitId            { get; set; }
    public string Code              { get; set; }
    public string Notes             { get; set; }
}

public class UpdateMeterDto
{
    public string  Code     { get; set; }
    public string? Notes    { get; set; }
    public bool    IsActive { get; set; }
}
