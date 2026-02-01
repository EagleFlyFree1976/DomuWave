namespace DomuWave.Application.Models;

public class PaymentMethodCreateUpdateDto
{
    public long? BookId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}