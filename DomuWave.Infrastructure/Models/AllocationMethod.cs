namespace DomuWave.Services.Models;

public enum AllocationMethod
{
    /// <summary>100% ripartito via tabella millesimale (default).</summary>
    Standard = 0,

    /// <summary>
    /// Y% via tabella millesimale + (100-Y)% via fattore = FloorWeight * Piano + InhabitantsWeight * NumeroAbitanti.
    /// </summary>
    Mixed = 1,
}
