namespace DomuWave.Services.Dto.Admin;

public class HiloAlignmentResultDto
{
    public string EntityType    { get; set; }
    public string TableName     { get; set; }
    public string IdColumn      { get; set; }
    public long   MaxId         { get; set; }
    public int    MaxLo         { get; set; }
    public int    ComputedNextHi { get; set; }
    public int?   OldNextHi     { get; set; }
    public int    FinalNextHi   { get; set; }
    public string Action        { get; set; }
}
