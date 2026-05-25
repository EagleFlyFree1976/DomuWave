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
    public string Email               { get; set; } = string.Empty;
}

public class ConfirmRegistrationDto
{
    public Guid RegistrationId { get; set; }
}

public class ConfirmRegistrationResultDto
{
    public string Token      { get; set; } = string.Empty;
    public int    UserId     { get; set; }
    public string Email      { get; set; } = string.Empty;
    public Guid   TenantId   { get; set; }
    public string TenantName { get; set; } = string.Empty;
}
