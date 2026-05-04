namespace DomuWave.Services.Dto.Building;

public class CreateBuildingDto
{
    public int     CondominiumId      { get; set; }
    public string  Name               { get; set; }
    public string? Description        { get; set; }
    public string? Code               { get; set; }
    public string? Address            { get; set; }
    public int?    YearOfConstruction { get; set; }
    public int?    NumberOfFloors     { get; set; }
    public bool    HasElevator        { get; set; }
    public bool    IsActive           { get; set; } = true;
}
