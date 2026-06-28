using System;

namespace DomuWave.Services.Dto.ElectronicInvoice;

/// <summary>
/// Parametri per il download massivo delle fatture passive di un condominio.
/// </summary>
public class SyncEInvoicesDto
{
    public int CondominiumId { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}
