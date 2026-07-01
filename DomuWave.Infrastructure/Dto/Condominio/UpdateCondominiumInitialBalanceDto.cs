namespace DomuWave.Services.Dto.Condominium;

/// <summary>
/// DTO per l'aggiornamento mirato del solo saldo iniziale di cassa del condominio.
/// </summary>
public class UpdateCondominiumInitialBalanceDto
{
    public decimal InitialBalance { get; set; }
}
