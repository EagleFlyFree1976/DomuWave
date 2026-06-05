using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.TenantDisplay;

public class TenantDisplaySettingsReadDto : TraceEntityDTO<int>
{
    /// <summary>0 = SoloColore, 1 = SegnoEsplicito.</summary>
    public int    AccountingSignConvention     { get; set; }

    /// <summary>Nome leggibile della convenzione (per comodità del client).</summary>
    public string AccountingSignConventionName { get; set; } = string.Empty;
}
