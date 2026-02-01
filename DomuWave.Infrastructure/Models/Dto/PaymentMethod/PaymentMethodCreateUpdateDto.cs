using MassTransit.Futures.Contracts;

namespace DomuWave.Services.Models.Dto.PaymentMethod;

public class PaymentMethodCreateUpdateDto
{
    public long BookId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}