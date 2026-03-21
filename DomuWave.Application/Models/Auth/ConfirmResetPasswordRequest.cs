using System.ComponentModel.DataAnnotations;

namespace DomuWave.Microservice.Models;

public class ConfirmResetPasswordRequest
{
    [Required]
    public string Token { get; set; }

    [Required]
    public string NewPassword { get; set; }
}
