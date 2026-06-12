using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.TenantDisplay;

public class TenantDisplaySettingsReadDto : TraceEntityDTO<int>
{
    /// <summary>0 = SoloColore, 1 = SegnoEsplicito.</summary>
    public int    AccountingSignConvention     { get; set; }

    /// <summary>Nome leggibile della convenzione (per comodità del client).</summary>
    public string AccountingSignConventionName { get; set; } = string.Empty;

    // ── Branding ───────────────────────────────────────────────────────────
    /// <summary>True se il tenant ha un logo configurato.</summary>
    public bool    HasLogo         { get; set; }

    /// <summary>URL relativo da cui scaricare il logo (con cache-buster), o null.</summary>
    public string? LogoUrl         { get; set; }

    /// <summary>Content-type del logo (es. image/png), o null.</summary>
    public string? LogoContentType { get; set; }
}
