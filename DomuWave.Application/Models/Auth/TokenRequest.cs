using System.ComponentModel.DataAnnotations;

namespace DomuWave.Microservice.Models;

public class TokenRequest
{
    [Required]
    public string Token { get; set; }
}
