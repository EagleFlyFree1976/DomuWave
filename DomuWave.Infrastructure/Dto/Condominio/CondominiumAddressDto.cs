namespace DomuWave.Services.Dto.Condominium;

/// <summary>
/// DTO per l'indirizzo di un condominio.
/// </summary>
public class CondominiumAddressDto
{
    public string Street { get; set; }
    public string StreetNumber { get; set; }
    public string PostalCode { get; set; }
    public string City { get; set; }
    public string Province { get; set; }
    public string Country { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}
