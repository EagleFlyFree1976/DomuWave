using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.Consumption;

public class ConsumptionReadingReadDto : TraceEntityDTO<int>
{
    public int       MeterId          { get; set; }
    public string    MeterCode        { get; set; }
    public bool      MeterIsDeleted   { get; set; }
    public int       UnitId           { get; set; }
    public string    UnitName         { get; set; }
    public int       FiscalYearId     { get; set; }
    public string    FiscalYearCode   { get; set; }
    public DateTime? InitialDate      { get; set; }
    public decimal   InitialValue     { get; set; }
    public DateTime? FinalDate        { get; set; }
    public decimal   FinalValue       { get; set; }
    public decimal   Consumption      { get; set; }
    public string    Notes            { get; set; }
    public int?      ChargeId         { get; set; }
    public int?      ChargeStatusId   { get; set; }
}

public class CreateConsumptionReadingDto
{
    public int       MeterId      { get; set; }
    public int       FiscalYearId { get; set; }
    public DateTime? InitialDate  { get; set; }
    public decimal   InitialValue { get; set; }
    public DateTime? FinalDate    { get; set; }
    public decimal   FinalValue   { get; set; }
    public string?   Notes        { get; set; }
}

public class UpdateConsumptionReadingDto
{
    public DateTime? InitialDate  { get; set; }
    public decimal   InitialValue { get; set; }
    public DateTime? FinalDate    { get; set; }
    public decimal   FinalValue   { get; set; }
    public string?   Notes        { get; set; }
}
