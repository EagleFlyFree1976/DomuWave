namespace DomuWave.Services.Dto.Auth;

public class SelfRegisterDto
{
    public string Email      { get; set; } = string.Empty;
    public string Password   { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
}

public class SelfRegisterResultDto
{
    public Guid   RegistrationId { get; set; }
    public string Email          { get; set; } = string.Empty;
    public string TenantName     { get; set; } = string.Empty;
}

public class CheckEmailDto
{
    public string Email { get; set; } = string.Empty;
}

public class CheckEmailResultDto
{
    public bool   IsExistingCondomino { get; set; }
    /// <summary>True se l'email appartiene già a un account amministratore (deve fare login, non registrarsi).</summary>
    public bool   IsExistingAdmin     { get; set; }
    public string Email               { get; set; } = string.Empty;
}

// Step 3: salva i dati del condominio, genera il token e invia la mail di verifica
public class RequestVerificationDto
{
    public Guid    RegistrationId   { get; set; }
    public string? CondominiumName  { get; set; }
    public string? CondominiumCode  { get; set; }
    public string? CondominiumCity  { get; set; }
    public string? CondominiumZip   { get; set; }
}

public class RequestVerificationResultDto
{
    public string Email { get; set; } = string.Empty;
}

public class ConfirmRegistrationDto
{
    // Verifica via token ricevuto per email
    public string Token { get; set; } = string.Empty;
}

public class ConfirmRegistrationResultDto
{
    public string Token      { get; set; } = string.Empty;
    public int    UserId     { get; set; }
    public string Email      { get; set; } = string.Empty;
    public Guid   TenantId   { get; set; }
    public string TenantName { get; set; } = string.Empty;
}
