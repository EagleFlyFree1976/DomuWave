using DomuWave.Services.Models.Dto.PaymentMethod;

namespace DomuWave.Application.Models;

public class PaymentMethodDto
{
    public PaymentMethodReadDto PaymentMethod { get; set; }
    public bool IsDefault { get; set; }
}