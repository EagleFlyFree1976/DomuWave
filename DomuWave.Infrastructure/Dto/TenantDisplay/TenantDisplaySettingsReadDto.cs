using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.TenantDisplay;

public class TenantDisplaySettingsReadDto : TraceEntityDTO<int>
{
    /// <summary>0 = SoloColore, 1 = SegnoEsplicito.</summary>
    public int    AccountingSignConvention     { get; set; }

    /// <summary>Nome leggibile della convenzione (per comodità del client).</summary>
    public string AccountingSignConventionName { get; set; } = string.Empty;

    // ── Logo ────────────────────────────────────────────────────────────────
    /// <summary>True se è presente un logo caricato per il tenant.</summary>
    public bool    HasLogo         { get; set; }

    /// <summary>Content-type del logo (es. image/png), null se assente.</summary>
    public string? LogoContentType { get; set; }

    /// <summary>URL autenticato del logo, con cache-busting; null se assente.</summary>
    public string? LogoUrl         { get; set; }
}
