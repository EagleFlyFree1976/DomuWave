using System.ComponentModel.DataAnnotations;

namespace DomuWave.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Il campo username è obbligatorio")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Il campo password è obbligatorio")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    public bool RememberMe { get; set; }
}