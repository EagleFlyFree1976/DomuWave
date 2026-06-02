namespace DomuWave.Services.Dto.TenantDisplay;

public class UpsertTenantDisplaySettingsDto
{
    /// <summary>0 = SoloColore (default), 1 = SegnoEsplicito.</summary>
    public int AccountingSignConvention { get; set; }
}
