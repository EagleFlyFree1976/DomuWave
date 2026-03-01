namespace DomuWave.Services.Dto.UnitOwner;

public class UserUnitOwnerDto
{
    public int    UnitId          { get; set; }
    public int    CondominiumId   { get; set; }
    public string? CondominiumName { get; set; }
    /// <summary>Guid del tenant, come stringa (es. "aabbcc00-...").</summary>
    public string TenantId        { get; set; } = string.Empty;
}
