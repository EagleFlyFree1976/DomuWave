namespace DomuWave.Services.Models;

public class PendingRegistration
{
    public virtual Guid     Id           { get; set; }
    public virtual string   Email        { get; set; } = string.Empty;
    public virtual string   PasswordHash { get; set; } = string.Empty;
    public virtual string   TenantName   { get; set; } = string.Empty;
    public virtual int      Status       { get; set; }
    public virtual DateTime CreatedAt    { get; set; }
    public virtual DateTime? ConfirmedAt { get; set; }
    public virtual DateTime ExpiresAt   { get; set; }

    public override int GetHashCode() => Id.GetHashCode();
}

public static class PendingRegistrationStatus
{
    public const int Pending   = 0;
    public const int Confirmed = 1;
    public const int Expired   = 2;
}
